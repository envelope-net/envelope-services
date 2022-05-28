using Envelope.Localization;
using Envelope.Logging;
using Envelope.Trace;

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
}
