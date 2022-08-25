using Envelope.Logging;
using Envelope.Trace;
using Envelope.Transactions;

namespace Envelope.Services.Transactions;

public partial class ServiceTransactionInterceptor : TransactionInterceptor
{
	public static IResult ExecuteAction(
		bool isReadOnly,
		ITraceInfo traceInfo,
		ITransactionController transactionController,
		Func<ITraceInfo, ITransactionController, IResult> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, IErrorMessage> onError,
		Action? @finally,
		bool disposeTransactionController = true)
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
			var actionResult = action(traceInfo, transactionController);
			result.MergeAllHasError(actionResult);

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			//if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			if (actionResult == null)
				throw new InvalidOperationException($"{nameof(actionResult)} == null");

			if (actionResult.HasError)
			{
				if (transactionController.TransactionResult != TransactionResult.Rollback)
					transactionController.ScheduleRollback(null);
			}
			else
			{
				if (transactionController.TransactionResult == TransactionResult.Commit)
					transactionController.TransactionCoordinator.Commit();
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

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			//if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			try
			{
				transactionController.TransactionCoordinator.TryRollback(ex);
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
			if (transactionController.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					transactionController.TransactionCoordinator.TryRollback(null);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						var errorMessage = 
							onError(
								traceInfo,
								rollbackEx,
								!string.IsNullOrWhiteSpace(transactionController.RollbackErrorInfo)
									? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionController.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
									: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo));

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

			if (disposeTransactionController)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionController.Dispose();
#else
					transactionController.Dispose();
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
		ITransactionController transactionController,
		Func<ITraceInfo, ITransactionController, IResult<T>> action,
		string? unhandledExceptionDetail,
		Func<ITraceInfo, Exception?, string?, IErrorMessage> onError,
		Action? @finally,
		bool disposeTransactionController = true)
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
			var actionResult = action(traceInfo, transactionController);
			result.MergeAllHasError(actionResult);

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			//if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			if (actionResult == null)
				throw new InvalidOperationException($"{nameof(actionResult)} == null");

			if (actionResult.HasError)
			{
				if (transactionController.TransactionResult != TransactionResult.Rollback)
					transactionController.ScheduleRollback(null);
			}
			else
			{
				if (transactionController.TransactionResult == TransactionResult.Commit)
					transactionController.TransactionCoordinator.Commit();
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

			if (isReadOnly && transactionController.TransactionResult != TransactionResult.None)
				throw new InvalidOperationException($"{nameof(isReadOnly)} == true | {nameof(transactionController.TransactionResult)} == {transactionController.TransactionResult}");

			//if (!isReadOnly && transactionController.TransactionResult == TransactionResult.None)
			//	throw new InvalidOperationException($"{nameof(isReadOnly)} == false | {nameof(transactionController.TransactionResult)} == {TransactionResult.None}");

			try
			{
				transactionController.TransactionCoordinator.TryRollback(ex);
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
			if (transactionController.TransactionResult == TransactionResult.Rollback)
			{
				try
				{
					transactionController.TransactionCoordinator.TryRollback(null);
				}
				catch (Exception rollbackEx)
				{
					try
					{
						var errorMessage = 
							onError(
								traceInfo,
								rollbackEx,
								!string.IsNullOrWhiteSpace(transactionController.RollbackErrorInfo)
									? $"{(!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? $"{unhandledExceptionDetail} " : "")}{transactionController.RollbackErrorInfo} {DefaultRollbackErrorInfo}"
									: (!string.IsNullOrWhiteSpace(unhandledExceptionDetail) ? unhandledExceptionDetail : DefaultRollbackErrorInfo));

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

			if (disposeTransactionController)
			{
				try
				{
#if NETSTANDARD2_0 || NETSTANDARD2_1
					transactionController.Dispose();
#else
					transactionController.Dispose();
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
