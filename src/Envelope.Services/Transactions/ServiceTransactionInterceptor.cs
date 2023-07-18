using Envelope.Logging;
using Envelope.Trace;
using Envelope.Transactions;

namespace Envelope.Services.Transactions;

public partial class ServiceTransactionInterceptor : TransactionInterceptor
{
	public static async Task<IResult> ExecuteActionAsync(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionController transactionController,
		Func<ITraceInfo, ITransactionController, string?, CancellationToken, Task<IResult>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task<IErrorMessage>> onError,
		Func<Task>? @finally,
		bool disposeTransactionController = true,
		CancellationToken cancellationToken = default)
	{
		traceInfo = TraceInfo.Create(traceInfo);
		var result = new ResultBuilder();

		if (action == null)
			return result.WithArgumentNullException(traceInfo, nameof(action));

		if (onError == null)
			return result.WithArgumentNullException(traceInfo, nameof(onError));

		if (transactionController == null)
			return result.WithArgumentNullException(traceInfo, nameof(transactionController));

		try
		{
			var actionResult = await action(traceInfo, transactionController, unhandledExceptionDetail, cancellationToken).ConfigureAwait(false);
			result.MergeAll(actionResult);

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			//if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			if (actionResult == null)
				throw new InvalidOperationException($"{nameof(actionResult)} == null");

			if (actionResult.HasTransactionRollbackError)
			{
				if (transactionController.TransactionResult != TransactionResult.Rollback)
					transactionController.ScheduleRollback(null);
			}
			else
			{
				if (transactionController.TransactionResult == TransactionResult.Commit)
					await transactionController.TransactionCoordinator.CommitAsync(cancellationToken).ConfigureAwait(false);
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

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			//if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			try
			{
				await transactionController.TransactionCoordinator.TryRollbackAsync(ex, cancellationToken).ConfigureAwait(false);
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
			if (transactionController.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					await transactionController.TransactionCoordinator.TryRollbackAsync(null, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						var errorMessage = 
							await onError(
								traceInfo,
								rollbackEx,
								!string.IsNullOrWhiteSpace(transactionController.RollbackErrorInfo)
									? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionController.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
									: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo)).ConfigureAwait(false);

						result.WithError(errorMessage);
					}
					catch (Exception logEx)
					{
#pragma warning disable CS8604 // Possible null reference argument.
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
						result.WithInvalidOperationException(
							traceInfo,
							!string.IsNullOrWhiteSpace(transactionController.RollbackErrorInfo)
									? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionController.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
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

			if (disposeTransactionController)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionController.Dispose();
#elif NET6_0_OR_GREATER
					await transactionController.DisposeAsync().ConfigureAwait(false);
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
		ITransactionController transactionController,
		Func<ITraceInfo, ITransactionController, string?, CancellationToken, Task<IResult<T>>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, Task<IErrorMessage>> onError,
		Func<Task>? @finally,
		bool disposeTransactionController = true,
		CancellationToken cancellationToken = default)
	{
		traceInfo = TraceInfo.Create(traceInfo);
		var result = new ResultBuilder<T>();

		if (action == null)
			return result.WithArgumentNullException(traceInfo, nameof(action));

		if (onError == null)
			return result.WithArgumentNullException(traceInfo, nameof(onError));

		if (transactionController == null)
			return result.WithArgumentNullException(traceInfo, nameof(transactionController));

		try
		{
			var actionResult = await action(traceInfo, transactionController, unhandledExceptionDetail, cancellationToken).ConfigureAwait(false);
			result.MergeAll(actionResult);

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			//if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			if (actionResult == null)
				throw new InvalidOperationException($"{nameof(actionResult)} == null");

			if (actionResult.HasTransactionRollbackError)
			{
				if (transactionController.TransactionResult != TransactionResult.Rollback)
					transactionController.ScheduleRollback(null);
			}
			else
			{
				if (transactionController.TransactionResult == TransactionResult.Commit) 
					await transactionController.TransactionCoordinator.CommitAsync(cancellationToken).ConfigureAwait(false);
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

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			//if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			try
			{
				await transactionController.TransactionCoordinator.TryRollbackAsync(ex, cancellationToken).ConfigureAwait(false);
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
			if (transactionController.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					await transactionController.TransactionCoordinator.TryRollbackAsync(null, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						var errorMessage = 
							await onError(
								traceInfo,
								rollbackEx,
								!string.IsNullOrWhiteSpace(transactionController.RollbackErrorInfo)
									? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionController.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
									: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo)).ConfigureAwait(false);

						result.WithError(errorMessage);
					}
					catch (Exception logEx)
					{
#pragma warning disable CS8604 // Possible null reference argument.
						result.WithInvalidOperationException(traceInfo, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo, logEx);
						result.WithInvalidOperationException(
							traceInfo,
							!string.IsNullOrWhiteSpace(transactionController.RollbackErrorInfo)
								? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionController.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
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

			if (disposeTransactionController)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionController.Dispose();
#elif NET6_0_OR_GREATER
					await transactionController.DisposeAsync().ConfigureAwait(false);
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
