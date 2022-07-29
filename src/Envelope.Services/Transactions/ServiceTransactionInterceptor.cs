using Envelope.Logging;
using Envelope.Trace;
using Envelope.Transactions;

namespace Envelope.Services.Transactions;

public partial class ServiceTransactionInterceptor : TransactionInterceptor
{
	public ServiceTransactionInterceptor(
		IServiceProvider serviceProvider,
		ITransactionManagerFactory transactionManagerFactory,
		Func<IServiceProvider, ITransactionManager, Task<ITransactionContext>> transactionContextFactory,
		Func<IServiceProvider, ITransactionManager, ITransactionContext> syncTransactionContextFactory)
		: base(serviceProvider, transactionManagerFactory, transactionContextFactory, syncTransactionContextFactory)
	{
	}

	public virtual async Task<IResult> ExecuteAsync(
		bool isReadOnly,
		ITraceInfo traceInfo,
		Func<ITraceInfo, ITransactionContext, CancellationToken, Task<IResult>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task<IErrorMessage>> onError,
		Func<Task>? @finally,
		bool throwOnError = true,
		CancellationToken cancellationToken = default)
		=> await ExecuteActionAsync(
				isReadOnly,
				traceInfo,
				await CreateTransactionContextAsync().ConfigureAwait(false),
				action,
				unhandledExceptionDetail,
				onError,
				@finally,
				true,
				cancellationToken).ConfigureAwait(false);

	public virtual async Task<IResult<T>> ExecuteAsync<T>(
		bool isReadOnly,
		ITraceInfo traceInfo,
		Func<ITraceInfo, ITransactionContext, CancellationToken, Task<IResult<T>>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task<IErrorMessage>> onError,
		Func<Task>? @finally,
		CancellationToken cancellationToken = default)
		=> await ExecuteActionAsync(
				isReadOnly,
				traceInfo,
				await CreateTransactionContextAsync().ConfigureAwait(false),
				action,
				unhandledExceptionDetail,
				onError,
				@finally,
				true,
				cancellationToken).ConfigureAwait(false);

	public static async Task<IResult> ExecuteActionAsync(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionContext transactionContext,
		Func<ITraceInfo, ITransactionContext, CancellationToken, Task<IResult>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task<IErrorMessage>> onError,
		Func<Task>? @finally,
		bool disposeTransactionContext = true,
		CancellationToken cancellationToken = default)
	{
		traceInfo = TraceInfo.Create(traceInfo);
		var result = new ResultBuilder();

		if (action == null)
			return result.WithArgumentNullException(traceInfo, nameof(action));

		if (onError == null)
			return result.WithArgumentNullException(traceInfo, nameof(onError));

		if (transactionContext == null)
			return result.WithArgumentNullException(traceInfo, nameof(transactionContext));

		try
		{
			var actionResult = await action(traceInfo, transactionContext, cancellationToken).ConfigureAwait(false);
			result.MergeHasError(actionResult);

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			//if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			if (actionResult == null)
				throw new InvalidOperationException($"{nameof(actionResult)} == null");

			if (actionResult.HasError)
			{
				if (transactionContext.TransactionResult != TransactionResult.Rollback)
					transactionContext.ScheduleRollback(null);
			}
			else
			{
				if (transactionContext.TransactionResult == TransactionResult.Commit)
					await transactionContext.TransactionManager.CommitAsync(cancellationToken).ConfigureAwait(false);
			}

			return result.Build();
		}
		catch (Exception ex)
		{
			try
			{
				var errorMessage = await onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo).ConfigureAwait(false);
				result.WithError(errorMessage);
			}
			catch (Exception logEx)
			{
#pragma warning disable CS8604 // Possible null reference argument.
				result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
				result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, ex);
#pragma warning restore CS8604 // Possible null reference argument.
			}

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			//if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			try
			{
				await transactionContext.TransactionManager.TryRollbackAsync(ex, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception rollbackEx)
			{
				try
				{
					var errorMessage = await onError(traceInfo, rollbackEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo).ConfigureAwait(false);
					result.WithError(errorMessage);
				}
				catch (Exception logEx)
				{
#pragma warning disable CS8604 // Possible null reference argument.
					result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
					result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo, rollbackEx);
#pragma warning restore CS8604 // Possible null reference argument.
				}
			}

			return result.Build();
		}
		finally
		{
			if (transactionContext.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					await transactionContext.TransactionManager.TryRollbackAsync(null, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						var errorMessage = 
							await onError(
								traceInfo,
								rollbackEx,
								!string.IsNullOrWhiteSpace(transactionContext.RollbackErrorInfo)
									? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionContext.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
									: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo)).ConfigureAwait(false);

						result.WithError(errorMessage);
					}
					catch (Exception logEx)
					{
#pragma warning disable CS8604 // Possible null reference argument.
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
						result.WithInvalidOperationException(
							traceInfo,
							!string.IsNullOrWhiteSpace(transactionContext.RollbackErrorInfo)
									? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionContext.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
									: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo));
#pragma warning restore CS8604 // Possible null reference argument.
					}
				}
			}

			if (@finally != null)
			{
				try
				{
					await @finally().ConfigureAwait(false);
				}
				catch (Exception finallyEx)
				{
					try
					{
						var errorMessage = await onError(traceInfo, finallyEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultFinallyInfo).ConfigureAwait(false);
						result.WithError(errorMessage);
					}
					catch (Exception logEx)
					{
#pragma warning disable CS8604 // Possible null reference argument.
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultFinallyInfo, finallyEx);
#pragma warning restore CS8604 // Possible null reference argument.
					}
				}
			}

			if (disposeTransactionContext)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionContext.Dispose();
#else
					await transactionContext.DisposeAsync().ConfigureAwait(false);
#endif
				}
				catch (Exception disposeEx)
				{
					try
					{
						var errorMessage = await onError(traceInfo, disposeEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultDisposeInfo).ConfigureAwait(false);
						result.WithError(errorMessage);
					}
					catch (Exception logEx)
					{
#pragma warning disable CS8604 // Possible null reference argument.
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultDisposeInfo, disposeEx);
#pragma warning restore CS8604 // Possible null reference argument.
					}
				}
			}
		}
	}

	public static async Task<IResult<T>> ExecuteActionAsync<T>(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionContext transactionContext,
		Func<ITraceInfo, ITransactionContext, CancellationToken, Task<IResult<T>>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task<IErrorMessage>> onError,
		Func<Task>? @finally,
		bool disposeTransactionContext = true,
		CancellationToken cancellationToken = default)
	{
		traceInfo = TraceInfo.Create(traceInfo);
		var result = new ResultBuilder<T>();

		if (action == null)
			return result.WithArgumentNullException(traceInfo, nameof(action));

		if (onError == null)
			return result.WithArgumentNullException(traceInfo, nameof(onError));

		if (transactionContext == null)
			return result.WithArgumentNullException(traceInfo, nameof(transactionContext));

		try
		{
			var actionResult = await action(traceInfo, transactionContext, cancellationToken).ConfigureAwait(false);
			result.MergeHasError(actionResult);

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			//if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			if (actionResult == null)
				throw new InvalidOperationException($"{nameof(actionResult)} == null");

			if (actionResult.HasError)
			{
				if (transactionContext.TransactionResult != TransactionResult.Rollback)
					transactionContext.ScheduleRollback(null);
			}
			else
			{
				if (transactionContext.TransactionResult == TransactionResult.Commit) 
					await transactionContext.TransactionManager.CommitAsync(cancellationToken).ConfigureAwait(false);
			}

			return result.WithData(actionResult.Data).Build();
		}
		catch (Exception ex)
		{
			try
			{
				var errorMessage = await onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo).ConfigureAwait(false);
				result.WithError(errorMessage);
			}
			catch (Exception logEx)
			{
#pragma warning disable CS8604 // Possible null reference argument.
				result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
				result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, ex);
#pragma warning restore CS8604 // Possible null reference argument.
			}

			if (isReadOnly && transactionContext.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionContext.TransactionResult)} == {transactionContext.TransactionResult}");

			//if (!isReadOnly && transactionContext.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionContext.TransactionResult)} == {TransactionResult.None}");

			try
			{
				await transactionContext.TransactionManager.TryRollbackAsync(ex, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception rollbackEx)
			{
				try
				{
					var errorMessage = await onError(traceInfo, rollbackEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo).ConfigureAwait(false);
					result.WithError(errorMessage);
				}
				catch (Exception logEx)
				{
#pragma warning disable CS8604 // Possible null reference argument.
					result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
					result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo, rollbackEx);
#pragma warning restore CS8604 // Possible null reference argument.
				}
			}

			return result.Build();
		}
		finally
		{
			if (transactionContext.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					await transactionContext.TransactionManager.TryRollbackAsync(null, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						var errorMessage = 
							await onError(
								traceInfo,
								rollbackEx,
								!string.IsNullOrWhiteSpace(transactionContext.RollbackErrorInfo)
									? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionContext.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
									: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo)).ConfigureAwait(false);

						result.WithError(errorMessage);
					}
					catch (Exception logEx)
					{
#pragma warning disable CS8604 // Possible null reference argument.
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
						result.WithInvalidOperationException(
							traceInfo,
							!string.IsNullOrWhiteSpace(transactionContext.RollbackErrorInfo)
								? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionContext.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
								: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo));
#pragma warning restore CS8604 // Possible null reference argument.
					}
				}
			}

			if (@finally != null)
			{
				try
				{
					await @finally().ConfigureAwait(false);
				}
				catch (Exception finallyEx)
				{
					try
					{
						var errorMessage = await onError(traceInfo, finallyEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultFinallyInfo).ConfigureAwait(false);
						result.WithError(errorMessage);
					}
					catch (Exception logEx)
					{
#pragma warning disable CS8604 // Possible null reference argument.
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultFinallyInfo, finallyEx);
#pragma warning restore CS8604 // Possible null reference argument.
					}
				}
			}

			if (disposeTransactionContext)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionContext.Dispose();
#else
					await transactionContext.DisposeAsync().ConfigureAwait(false);
#endif
				}
				catch (Exception disposeEx)
				{
					try
					{
						var errorMessage = await onError(traceInfo, disposeEx, DefaultDisposeInfo).ConfigureAwait(false);
						result.WithError(errorMessage);
					}
					catch (Exception logEx)
					{
#pragma warning disable CS8604 // Possible null reference argument.
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultDisposeInfo, disposeEx);
#pragma warning restore CS8604 // Possible null reference argument.
					}
				}
			}
		}
	}
}
