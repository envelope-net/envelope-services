﻿using Microsoft.Extensions.Logging;
using Envelope.Model;
using Envelope.Logging;
using Envelope.Trace;
using System.Runtime.CompilerServices;

namespace Envelope.Services;

public abstract class QueryRepositoryBase<TEntity> : IQueryRepository<TEntity>
	where TEntity : IQueryEntity
{
	protected ILogger Logger { get; }

	public QueryRepositoryBase(ILogger logger)
	{
		Logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public abstract IQueryable<TEntity> AsQueryable();

	public MethodLogScope CreateScope(
		MethodLogScope methodLogScope,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
		=> CreateScope(methodLogScope?.TraceInfo!, methodParameters, memberName, sourceFilePath, sourceLineNumber);

	public MethodLogScope CreateScope(
		ITraceInfo previousTraceInfo,
		IEnumerable<MethodParameter>? methodParameters = null,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string sourceFilePath = "",
		[CallerLineNumber] int sourceLineNumber = 0)
	{
		if (previousTraceInfo == null)
			throw new ArgumentNullException(nameof(previousTraceInfo));

		var traceInfo =
			new TraceInfoBuilder(
				previousTraceInfo?.SourceSystemName!,
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
