using Microsoft.Extensions.Logging;
using Envelope.Model;
using Envelope.Logging;
using Envelope.Trace;
using System.Runtime.CompilerServices;

namespace Envelope.Services;

public abstract class ServiceBase<TEntity> : IService<TEntity>
	where TEntity : IEntity
{
	protected ILogger Logger { get; }

	public ServiceBase(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public MethodLogScope CreateScope(
		MethodLogScope? methodLogScope,
		string? sourceSystemName = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> CreateScope(methodLogScope?.TraceInfo, sourceSystemName, methodParameters, memberName, sourceFilePath, sourceLineNumber);

	public MethodLogScope CreateScope(
		ITraceInfo? previousTraceInfo,
		string? sourceSystemName = null,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
	{
		var traceInfo =
			new TraceInfoBuilder(
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
			[nameof(ILogMessage.TraceInfo.TraceFrame.MethodCallId)] = traceInfo.TraceFrame.MethodCallId,
			[nameof(ILogMessage.TraceInfo.CorrelationId)] = traceInfo.CorrelationId
		});

		var scope = new MethodLogScope(traceInfo, disposable);
		return scope;
	}
}
