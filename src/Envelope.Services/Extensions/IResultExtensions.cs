using Envelope.Extensions;
using Envelope.Localization;
using Envelope.Logging;
using Envelope.Trace;
using Envelope.Validation;
using Envelope.Validation.Results;
using Microsoft.Extensions.Logging;

namespace Envelope.Services;

public static class IResultExtensions
{
	public static TObject WithArgumentException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_Arg)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithArgumentNullException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string paramName,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_ArgNull)
					.ExceptionInfo(ex)
					.InternalMessage($"{paramName} == null")
					.Detail(ex == null ? null : $"{paramName} == null"))
			.Build();

	public static TObject WithArgumentOutOfRangeException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_ArgRange)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithInvalidOperationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithNotImplementedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_NotImpl)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithNotSupportedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_NotSupp)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithApplicationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				x => x
					.LogCode(LogCode.Ex_App)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithForbiddenException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		IApplicationResourcesProvider applicationResourcesProvider,
		string internalMessage,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		IApplicationResourcesProvider applicationResourcesProvider,
		string internalMessage,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string clientMessage,
		string? internalMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithWarn(
				traceInfo,
				x => x
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithClientWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string clientMessage,
		string? internalMessage = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithWarn(
				traceInfo,
				x => x
					.ClientMessage(clientMessage)
					.InternalMessage(internalMessage))
			.Build();

	public static TObject WithArgumentException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_Arg)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithArgumentNullException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string paramName,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_ArgRange)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithInvalidOperationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithNotImplementedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_NotImpl)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithNotSupportedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_NotSupp)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithApplicationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_App)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithForbiddenException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		IApplicationResourcesProvider applicationResourcesProvider,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		IApplicationResourcesProvider applicationResourcesProvider,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string clientMessage,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				traceInfo,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(clientMessage))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		Action<LogMessageBuilder>? logMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithWarn(
				traceInfo,
				(x => x
					.ExceptionInfo(ex))
				+ logMessageConfigurator)
			.Build();

	public static TObject WithClientWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		ITraceInfo traceInfo,
		string clientMessage,
		Action<LogMessageBuilder>? logMessageConfigurator = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithWarn(
				traceInfo,
				(x => x
					.ClientMessage(clientMessage))
				+ logMessageConfigurator)
			.Build();

	// ************************** METHOD SCOPE **************************



	public static TObject WithArgumentException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_Arg)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithArgumentNullException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string paramName,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_ArgNull)
					.ExceptionInfo(ex)
					.InternalMessage($"{paramName} == null")
					.Detail(ex == null ? null : $"{paramName} == null"))
			.Build();

	public static TObject WithArgumentOutOfRangeException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_ArgRange)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithInvalidOperationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithNotImplementedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_NotImpl)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithNotSupportedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_NotSupp)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithApplicationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				x => x
					.LogCode(LogCode.Ex_App)
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithForbiddenException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		IApplicationResourcesProvider applicationResourcesProvider,
		string internalMessage,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		IApplicationResourcesProvider applicationResourcesProvider,
		string internalMessage,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string clientMessage,
		string? internalMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string internalMessage,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithWarn(
				scope,
				x => x
					.ExceptionInfo(ex)
					.InternalMessage(internalMessage)
					.Detail(ex == null ? null : internalMessage))
			.Build();

	public static TObject WithClientWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string clientMessage,
		string? internalMessage = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithWarn(
				scope,
				x => x
					.ClientMessage(clientMessage)
					.InternalMessage(internalMessage))
			.Build();

	public static TObject WithArgumentException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_Arg)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithArgumentNullException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string paramName,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_ArgRange)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithInvalidOperationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithNotImplementedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_NotImpl)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithNotSupportedException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_NotSupp)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithApplicationException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_App)
					.ExceptionInfo(ex))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithForbiddenException<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		IApplicationResourcesProvider applicationResourcesProvider,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		IApplicationResourcesProvider applicationResourcesProvider,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		string? clientMessage = null,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
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
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string clientMessage,
		Action<ErrorMessageBuilder>? errorMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithError(
				scope,
				(x => x
					.LogCode(LogCode.Ex_InvOp)
					.ExceptionInfo(ex)
					.ClientMessage(clientMessage))
				+ errorMessageConfigurator)
			.Build();

	public static TObject WithWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		Action<LogMessageBuilder>? logMessageConfigurator,
		Exception? ex = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithWarn(
				scope,
				(x => x
					.ExceptionInfo(ex))
				+ logMessageConfigurator)
			.Build();

	public static TObject WithClientWarning<TBuilder, TObject>(
		this ResultBuilderBase<TBuilder, TObject> commandResultBuilder,
		MethodLogScope scope,
		string clientMessage,
		Action<LogMessageBuilder>? logMessageConfigurator = null)
		where TBuilder : ResultBuilderBase<TBuilder, TObject>
		where TObject : IResult
		=> commandResultBuilder
			.WithWarn(
				scope,
				(x => x
					.ClientMessage(clientMessage))
				+ logMessageConfigurator)
			.Build();

	public static bool MergeHasError(this IResult result, ITraceInfo traceInfo, IValidationResult validationResult)
	{
		if (result == null)
			throw new ArgumentNullException(nameof(result));

		if (validationResult == null)
			throw new ArgumentNullException(nameof(validationResult));

		foreach (var failure in validationResult.Errors)
		{
			if (failure.Severity == ValidationSeverity.Error)
			{
				var errorMessage = ValidationFailureToErrorMessage(traceInfo, failure);
				result.ErrorMessages.Add(errorMessage);
			}
			else
			{
				var warnigMessage = ValidationFailureToWarningMessage(traceInfo, failure);
				result.WarningMessages.Add(warnigMessage);
			}
		}

		return result.HasError;
	}

	public static bool MergeHasError<TResultBuilder>(this TResultBuilder resultBuilder, ITraceInfo traceInfo, IValidationResult validationResult)
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
				var errorMessage = ValidationFailureToErrorMessage(traceInfo, failure);
				resultBuilder.AddError(errorMessage);
			}
			else
			{
				var warnigMessage = ValidationFailureToWarningMessage(traceInfo, failure);
				resultBuilder.AddWarning(warnigMessage);
			}
		}

		return resultBuilder.HasAnyError();
	}

	private static IErrorMessage ValidationFailureToErrorMessage(ITraceInfo traceInfo, IBaseValidationFailure failure)
	{
		if (failure == null)
			throw new ArgumentNullException(nameof(failure));

		var errorMessageBuilder =
			new ErrorMessageBuilder(traceInfo)
				.LogLevel(LogLevel.Error)
				.ValidationFailure(failure, true)
				.ClientMessage(failure.Message, true) //TODO read from settings when using MessageWithPropertyName
				.Detail(failure.DetailInfo)
				.PropertyName(string.IsNullOrWhiteSpace(failure.ObjectPath.PropertyName) ? null : failure.ObjectPath.ToString()?.TrimPrefix("_."), !string.IsNullOrWhiteSpace(failure.ObjectPath.PropertyName));

		return errorMessageBuilder.Build();
	}

	private static ILogMessage ValidationFailureToWarningMessage(ITraceInfo traceInfo, IBaseValidationFailure failure)
	{
		if (failure == null)
			throw new ArgumentNullException(nameof(failure));

		var logMessageBuilder =
			new LogMessageBuilder(traceInfo)
				.LogLevel(LogLevel.Warning)
				.ValidationFailure(failure, true)
				.ClientMessage(failure.Message, true) //TODO read from settings when using MessageWithPropertyName
				.Detail(failure.DetailInfo)
				.PropertyName(string.IsNullOrWhiteSpace(failure.ObjectPath.PropertyName) ? null : failure.ObjectPath.ToString()?.TrimPrefix("_."), !string.IsNullOrWhiteSpace(failure.ObjectPath.PropertyName));

		return logMessageBuilder.Build();
	}
}
