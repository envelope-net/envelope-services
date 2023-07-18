using Envelope.Extensions;
using Envelope.Localization;
using Envelope.Logging;
using Envelope.Trace;
using Envelope.Validation;
using Microsoft.Extensions.Logging;

namespace Envelope.Services;

public static class IResultExtensions
{
	public static TResult ToDto<TResult>(
		this TResult result,
		params string[] ignoredPropterties)
		where TResult : IResult
	{
		if (result == null)
			throw new ArgumentNullException(nameof(result));

		for (int i = 0; i < result.SuccessMessages.Count; i++)
			result.SuccessMessages[i] = result.SuccessMessages[i].ToDto(ignoredPropterties);

		for (int i = 0; i < result.WarningMessages.Count; i++)
			result.WarningMessages[i] = result.WarningMessages[i].ToDto(ignoredPropterties);

		for (int i = 0; i < result.ErrorMessages.Count; i++)
			result.ErrorMessages[i] = result.ErrorMessages[i].ToDto(ignoredPropterties);

		return result;
	}

	public static TResult ToClientDto<TResult>(this TResult result)
		where TResult : IResult
	{
		if (result == null)
			throw new ArgumentNullException(nameof(result));

		for (int i = 0; i < result.SuccessMessages.Count; i++)
			result.SuccessMessages[i] = result.SuccessMessages[i].ToClientDto();

		for (int i = 0; i < result.WarningMessages.Count; i++)
			result.WarningMessages[i] = result.WarningMessages[i].ToClientDto();

		for (int i = 0; i < result.ErrorMessages.Count; i++)
			result.ErrorMessages[i] = result.ErrorMessages[i].ToClientDto();

		return result;
	}

	public static TObject ToDto<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		params string[] ignoredPropterties)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
	{
		if (resultBuilder == null)
			throw new ArgumentNullException(nameof(resultBuilder));

		var result = resultBuilder.Build();
		return result.ToDto(ignoredPropterties);
	}

	public static TObject ToClientDto<TBuilder, TObject>(this ResultBuilderBase<TBuilder, TObject> resultBuilder)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
	{
		if (resultBuilder == null)
			throw new ArgumentNullException(nameof(resultBuilder));

		var result = resultBuilder.Build();
		return result.ToClientDto();
	}

	public static TObject WithArgumentException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_Arg)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithArgumentNullException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string paramName,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_ArgNull)
					.ExceptionInfo(ex)
					.InternalMessage($"{paramName} == null")
					.Detail(ex == null ? null : $"{paramName} == null"))
			.Build();

	public static TObject WithArgumentOutOfRangeException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_ArgRange)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithInvalidOperationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithNotImplementedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_NotImpl)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithNotSupportedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_NotSupp)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithApplicationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_App)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithForbiddenException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		IApplicationResourcesProvider applicationResourcesProvider,
		string internalMessage,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(string.IsNullOrWhiteSpace(clientMessage) ? applicationResourcesProvider.ApplicationResources.DataForbiddenException : clientMessage)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithNotFoundException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		IApplicationResourcesProvider applicationResourcesProvider,
		string internalMessage,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(string.IsNullOrWhiteSpace(clientMessage) ? applicationResourcesProvider.ApplicationResources.DataNotFoundException : clientMessage)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithClientException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string clientMessage,
		string? internalMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(clientMessage)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithWarn(
				traceInfo,
				x => x
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithClientWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string clientMessage,
		string? internalMessage = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithWarn(
				traceInfo,
				x => x
					.ClientMessage(clientMessage)
					.InternalMessage(internalMessage))
			.Build();

	public static TObject WithArgumentException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_Arg)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithArgumentNullException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string paramName,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_ArgNull)
					.ExceptionInfo(ex)
					.InternalMessage($"{paramName} == null")
					.Detail(ex == null ? null : $"{paramName} == null"))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithArgumentOutOfRangeException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_ArgRange)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithInvalidOperationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithNotImplementedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_NotImpl)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithNotSupportedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_NotSupp)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithApplicationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_App)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithForbiddenException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		IApplicationResourcesProvider applicationResourcesProvider,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(string.IsNullOrWhiteSpace(clientMessage)
						? applicationResourcesProvider.ApplicationResources.DataForbiddenException
						: clientMessage))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithNotFoundException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		IApplicationResourcesProvider applicationResourcesProvider,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(string.IsNullOrWhiteSpace(clientMessage)
						? applicationResourcesProvider.ApplicationResources.DataNotFoundException
					: clientMessage))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithClientException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string clientMessage,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(clientMessage))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		Action<LogMessageBuilder>? logMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithWarn(
				traceInfo,
				(x => x
					.ExceptionInfo(ex))
				+ logMessageConfigurator)
			.Build();

	public static TObject WithClientWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		ITraceInfo traceInfo,
		string clientMessage,
		Action<LogMessageBuilder>? logMessageConfigurator = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithWarn(
				traceInfo,
				(x => x
					.ClientMessage(clientMessage))
				+ logMessageConfigurator)
			.Build();

	// ************************** METHOD SCOPE **************************



	public static TObject WithArgumentException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_Arg)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithArgumentNullException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string paramName,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_ArgNull)
					.ExceptionInfo(ex)
					.InternalMessage($"{paramName} == null")
					.Detail(ex == null ? null : $"{paramName} == null"))
			.Build();

	public static TObject WithArgumentOutOfRangeException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_ArgRange)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithInvalidOperationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithNotImplementedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_NotImpl)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithNotSupportedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_NotSupp)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithApplicationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_App)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithForbiddenException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		IApplicationResourcesProvider applicationResourcesProvider,
		string internalMessage,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(string.IsNullOrWhiteSpace(clientMessage) ? applicationResourcesProvider.ApplicationResources.DataForbiddenException : clientMessage)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithNotFoundException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		IApplicationResourcesProvider applicationResourcesProvider,
		string internalMessage,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(string.IsNullOrWhiteSpace(clientMessage) ? applicationResourcesProvider.ApplicationResources.DataNotFoundException : clientMessage)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithClientException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string clientMessage,
		string? internalMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(clientMessage)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithWarn(
				scope,
				x => x
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithClientWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string clientMessage,
		string? internalMessage = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithWarn(
				scope,
				x => x
					.ClientMessage(clientMessage)
					.InternalMessage(internalMessage))
			.Build();

	public static TObject WithArgumentException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_Arg)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithArgumentNullException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string paramName,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_ArgNull)
					.ExceptionInfo(ex)
					.InternalMessage($"{paramName} == null")
					.Detail(ex == null ? null : $"{paramName} == null"))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithArgumentOutOfRangeException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_ArgRange)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithInvalidOperationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithNotImplementedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_NotImpl)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithNotSupportedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_NotSupp)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithApplicationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_App)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithForbiddenException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		IApplicationResourcesProvider applicationResourcesProvider,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(string.IsNullOrWhiteSpace(clientMessage)
						? applicationResourcesProvider.ApplicationResources.DataForbiddenException
						: clientMessage))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithNotFoundException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		IApplicationResourcesProvider applicationResourcesProvider,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(string.IsNullOrWhiteSpace(clientMessage)
						? applicationResourcesProvider.ApplicationResources.DataNotFoundException
					: clientMessage))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithClientException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string clientMessage,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(clientMessage))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		Action<LogMessageBuilder>? logMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithWarn(
				scope,
				(x => x
					.ExceptionInfo(ex))
				+ logMessageConfigurator)
			.Build();

	public static TObject WithClientWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> resultBuilder,
		MethodLogScope scope,
		string clientMessage,
		Action<LogMessageBuilder>? logMessageConfigurator = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> resultBuilder
			.WithWarn(
				scope,
				(x => x
					.ClientMessage(clientMessage))
				+ logMessageConfigurator)
			.Build();

	public static bool MergeHasError(
		this IResult result,
		ITraceInfo traceInfo,
		IValidationResult validationResult,
		bool withPropertyName)
	{
		if (result == null)
			throw new ArgumentNullException(nameof(result));

		if (validationResult == null)
			throw new ArgumentNullException(nameof(validationResult));

		foreach (var failure in validationResult.Errors)
		{
			if (failure.Severity == ValidationSeverity.Error)
			{
				var errorMessage = ValidationFailureToErrorMessage(traceInfo, failure, withPropertyName);
				result.ErrorMessages.Add(errorMessage);
			}
			else
			{
				var warnigMessage = ValidationFailureToWarningMessage(traceInfo, failure, withPropertyName);
				result.WarningMessages.Add(warnigMessage);
			}
		}

		return result.HasError;
	}

	public static bool MergeHasTransactionRollbackError(
		this IResult result,
		ITraceInfo traceInfo,
		IValidationResult validationResult,
		bool withPropertyName)
	{
		if (result == null)
			throw new ArgumentNullException(nameof(result));

		if (validationResult == null)
			throw new ArgumentNullException(nameof(validationResult));

		foreach (var failure in validationResult.Errors)
		{
			if (failure.Severity == ValidationSeverity.Error)
			{
				var errorMessage = ValidationFailureToErrorMessage(traceInfo, failure, withPropertyName);
				result.ErrorMessages.Add(errorMessage);
			}
			else
			{
				var warnigMessage = ValidationFailureToWarningMessage(traceInfo, failure, withPropertyName);
				result.WarningMessages.Add(warnigMessage);
			}
		}

		return result.HasTransactionRollbackError;
	}

	public static bool MergeHasError(
		this IResult result,
		ITraceInfo traceInfo,
		System.Xml.Schema.ValidationEventArgs[] xmlValidationArgs)
	{
		if (result == null)
			throw new ArgumentNullException(nameof(result));

		if (0 < xmlValidationArgs?.Length)
		{
			foreach (var xmlValidationArg in xmlValidationArgs)
			{
				if (xmlValidationArg.Severity == System.Xml.Schema.XmlSeverityType.Error)
					result.ErrorMessages.Add(xmlValidationArg.ToErrorMessage(traceInfo));
				else
					result.WarningMessages.Add(xmlValidationArg.ToLogMessage(traceInfo));
			}
		}

		return result.HasError;
	}

	public static bool MergeHasTransactionRollbackError(
		this IResult result,
		ITraceInfo traceInfo,
		System.Xml.Schema.ValidationEventArgs[] xmlValidationArgs)
	{
		if (result == null)
			throw new ArgumentNullException(nameof(result));

		if (0 < xmlValidationArgs?.Length)
		{
			foreach (var xmlValidationArg in xmlValidationArgs)
			{
				if (xmlValidationArg.Severity == System.Xml.Schema.XmlSeverityType.Error)
					result.ErrorMessages.Add(xmlValidationArg.ToErrorMessage(traceInfo));
				else
					result.WarningMessages.Add(xmlValidationArg.ToLogMessage(traceInfo));
			}
		}

		return result.HasTransactionRollbackError;
	}

	public static bool MergeHasError<TResultBuilder>(
		this TResultBuilder resultBuilder,
		ITraceInfo traceInfo,
		IValidationResult validationResult, bool withPropertyName)
		where TResultBuilder : IResultBuilder
	{
		if (resultBuilder == null)
			throw new ArgumentNullException(nameof(resultBuilder));

		if (validationResult == null)
			throw new ArgumentNullException(nameof(validationResult));

		foreach (var failure in validationResult.Errors)
		{
			if (failure.Severity == ValidationSeverity.Error)
			{
				var errorMessage = ValidationFailureToErrorMessage(traceInfo, failure, withPropertyName);
				resultBuilder.AddError(errorMessage);
			}
			else
			{
				var warnigMessage = ValidationFailureToWarningMessage(traceInfo, failure, withPropertyName);
				resultBuilder.AddWarning(warnigMessage);
			}
		}

		return resultBuilder.HasAnyError();
	}

	public static bool MergeHasTransactionRollbackError<TResultBuilder>(
		this TResultBuilder resultBuilder,
		ITraceInfo traceInfo,
		IValidationResult validationResult, bool withPropertyName)
		where TResultBuilder : IResultBuilder
	{
		if (resultBuilder == null)
			throw new ArgumentNullException(nameof(resultBuilder));

		if (validationResult == null)
			throw new ArgumentNullException(nameof(validationResult));

		foreach (var failure in validationResult.Errors)
		{
			if (failure.Severity == ValidationSeverity.Error)
			{
				var errorMessage = ValidationFailureToErrorMessage(traceInfo, failure, withPropertyName);
				resultBuilder.AddError(errorMessage);
			}
			else
			{
				var warnigMessage = ValidationFailureToWarningMessage(traceInfo, failure, withPropertyName);
				resultBuilder.AddWarning(warnigMessage);
			}
		}

		return resultBuilder.HasAnyTransactionRollbackError();
	}

	public static bool MergeHasError<TResultBuilder>(
		this TResultBuilder resultBuilder,
		ITraceInfo traceInfo,
		System.Xml.Schema.ValidationEventArgs[] xmlValidationArgs)
		where TResultBuilder : IResultBuilder
	{
		if (resultBuilder == null)
			throw new ArgumentNullException(nameof(resultBuilder));

		if (0 < xmlValidationArgs?.Length)
		{
			foreach (var xmlValidationArg in xmlValidationArgs)
			{
				if (xmlValidationArg.Severity == System.Xml.Schema.XmlSeverityType.Error)
					resultBuilder.AddError(xmlValidationArg.ToErrorMessage(traceInfo));
				else
					resultBuilder.AddWarning(xmlValidationArg.ToLogMessage(traceInfo));
			}
		}

		return resultBuilder.HasAnyError();
	}

	public static bool MergeHasAnyTransactionRollbackError<TResultBuilder>(
		this TResultBuilder resultBuilder,
		ITraceInfo traceInfo,
		System.Xml.Schema.ValidationEventArgs[] xmlValidationArgs)
		where TResultBuilder : IResultBuilder
	{
		if (resultBuilder == null)
			throw new ArgumentNullException(nameof(resultBuilder));

		if (0 < xmlValidationArgs?.Length)
		{
			foreach (var xmlValidationArg in xmlValidationArgs)
			{
				if (xmlValidationArg.Severity == System.Xml.Schema.XmlSeverityType.Error)
					resultBuilder.AddError(xmlValidationArg.ToErrorMessage(traceInfo));
				else
					resultBuilder.AddWarning(xmlValidationArg.ToLogMessage(traceInfo));
			}
		}

		return resultBuilder.HasAnyTransactionRollbackError();
	}

	private static IErrorMessage ValidationFailureToErrorMessage(ITraceInfo traceInfo, IBaseValidationFailure failure, bool withPropertyName)
	{
		if (failure == null)
			throw new ArgumentNullException(nameof(failure));

		var errorMessageBuilder =
			new ErrorMessageBuilder(traceInfo)
				.LogLevel(LogLevel.Error)
				.ValidationFailure(failure, true)
				.ClientMessage(withPropertyName ? failure.MessageWithPropertyName : failure.Message, true)
				.Detail(failure.DetailInfo)
				.PropertyName(string.IsNullOrWhiteSpace(failure.ObjectPath.PropertyName) ? null : failure.ObjectPath.ToString()?.TrimPrefix("_."), !string.IsNullOrWhiteSpace(failure.ObjectPath.PropertyName));

		return errorMessageBuilder.Build();
	}

	private static ILogMessage ValidationFailureToWarningMessage(ITraceInfo traceInfo, IBaseValidationFailure failure, bool withPropertyName)
	{
		if (failure == null)
			throw new ArgumentNullException(nameof(failure));

		var logMessageBuilder =
			new LogMessageBuilder(traceInfo)
				.LogLevel(LogLevel.Warning)
				.ValidationFailure(failure, true)
				.ClientMessage(withPropertyName ? failure.MessageWithPropertyName : failure.Message, true)
				.Detail(failure.DetailInfo)
				.PropertyName(string.IsNullOrWhiteSpace(failure.ObjectPath.PropertyName) ? null : failure.ObjectPath.ToString()?.TrimPrefix("_."), !string.IsNullOrWhiteSpace(failure.ObjectPath.PropertyName));

		return logMessageBuilder.Build();
	}
}
