using Microsoft.Extensions.Logging;
using Envelope.Model;
using Envelope.Logging;
using Envelope.Trace;
using System.Runtime.CompilerServices;

namespace Envelope.Services;

public abstract class ServiceBase<TIdentity> : IService
	where TIdentity : struct
{
	protected ILogger Logger { get; }

	public ServiceBase(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public MethodLogScope<TIdentity> CreateScope(
		MethodLogScope<TIdentity>? methodLogScope,
		string? sourceSystemName = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> CreateScope(methodLogScope?.TraceInfo, sourceSystemName, methodParameters, memberName, sourceFilePath, sourceLineNumber);

	public MethodLogScope<TIdentity> CreateScope(
		ITraceInfo<TIdentity>? previousTraceInfo,
		string? sourceSystemName = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
	{
		var traceInfo =
			new TraceInfoBuilder<TIdentity>(
				sourceSystemName ?? previousTraceInfo?.SourceSystemName!,
				new TraceFrameBuilder(previousTraceInfo?.TraceFrame)
					.CallerMemberName(memberName)
					.CallerFilePath(sourceFilePath)
					.CallerLineNumber(sourceLineNumber == 0 ? (int?)null : sourceLineNumber)
					.MethodParameters(methodParameters)
					.Build(),
				previousTraceInfo)
				.Build();

		var disposable = Logger.BeginScope(new Dictionary<string, Guid?>
		{
			[nameof(ILogMessage<TIdentity>.TraceInfo.TraceFrame.MethodCallId)] = traceInfo.TraceFrame.MethodCallId,
			[nameof(ILogMessage<TIdentity>.TraceInfo.CorrelationId)] = traceInfo.CorrelationId
		});

		var scope = new MethodLogScope<TIdentity>(traceInfo, disposable);
		return scope;
	}
}
