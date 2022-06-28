using Envelope.Logging;
using Envelope.Trace;
using Envelope.Transactions;

namespace Envelope.Services.Transactions;

public partial class ServiceTransactionInterceptor : TransactionInterceptor
{
	public virtual IResult Execute(
		bool isReadOnly,
		ITraceInfo traceInfo,
		Func<ITraceInfo, ITransactionContext, IResult> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, IErrorMessage> onError,
		Action? @finally,
		bool throwOnError = true)
		=> ExecuteAction(
				isReadOnly,
				traceInfo,
				CreateTransactionContext(),
				action,
				unhandledExceptionDetail,
				onError,
				@finally,
				true);

	public virtual IResult<T> Execute<T>(
		bool isReadOnly,
		ITraceInfo traceInfo,
		Func<ITraceInfo, ITransactionContext, IResult<T>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, IErrorMessage> onError,
		Action? @finally)
		=> ExecuteAction(
				isReadOnly,
				traceInfo,
				CreateTransactionContext(),
				action,
				unhandledExceptionDetail,
				onError,
				@finally,
				true);

	public static IResult ExecuteAction(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionContext transactionContext,
		Func<ITraceInfo, ITransactionContext, IResult> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, IErrorMessage> onError,
		Action? @finally,
		bool disposeTransactionContext = true)
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
			var actionResult = action(traceInfo, transactionContext);
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
					transactionContext.TransactionManager.Commit();
			}

			return result.Build();
		}
		catch (Exception ex)
		{
			try
			{
				var errorMessage = onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo);
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
				transactionContext.TransactionManager.TryRollback(ex);
			}
			catch (Exception rollbackEx)
			{
				try
				{
					var errorMessage = onError(traceInfo, rollbackEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo);
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
					transactionContext.TransactionManager.TryRollback(null);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						var errorMessage = 
							onError(
								traceInfo,
								rollbackEx,
								!string.IsNullOrWhiteSpace(transactionContext.RollbackErrorInfo)
									? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionContext.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
									: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo));

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
					@finally();
				}
				catch (Exception finallyEx)
				{
					try
					{
						var errorMessage = onError(traceInfo, finallyEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultFinallyInfo);
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
					transactionContext.Dispose();
#endif
				}
				catch (Exception disposeEx)
				{
					try
					{
						var errorMessage = onError(traceInfo, disposeEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultDisposeInfo);
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

	public static IResult<T> ExecuteAction<T>(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionContext transactionContext,
		Func<ITraceInfo, ITransactionContext, IResult<T>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, IErrorMessage> onError,
		Action? @finally,
		bool disposeTransactionContext = true)
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
			var actionResult = action(traceInfo, transactionContext);
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
					transactionContext.TransactionManager.Commit();
			}

			return result.WithData(actionResult.Data).Build();
		}
		catch (Exception ex)
		{
			try
			{
				var errorMessage = onError(traceInfo, ex, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : UnhandledExceptionInfo);
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
				transactionContext.TransactionManager.TryRollback(ex);
			}
			catch (Exception rollbackEx)
			{
				try
				{
					var errorMessage = onError(traceInfo, rollbackEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo);
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
					transactionContext.TransactionManager.TryRollback(null);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						var errorMessage = 
							onError(
								traceInfo,
								rollbackEx,
								!string.IsNullOrWhiteSpace(transactionContext.RollbackErrorInfo)
									? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionContext.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
									: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo));

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
					@finally();
				}
				catch (Exception finallyEx)
				{
					try
					{
						var errorMessage = onError(traceInfo, finallyEx, !string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultFinallyInfo);
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
					transactionContext.Dispose();
#endif
				}
				catch (Exception disposeEx)
				{
					try
					{
						var errorMessage = onError(traceInfo, disposeEx, DefaultDisposeInfo);
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
