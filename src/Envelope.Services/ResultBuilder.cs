using Microsoft.Extensions.Logging;
using Envelope.Logging;
using Envelope.Trace;

namespace Envelope.Services;

public interface IResultBuilder<TBuilder, TObject, TIdentity>
	where TBuilder : IResultBuilder<TBuilder, TObject, TIdentity>
	where TObject : IResult<TIdentity>
	where TIdentity : struct
{
	TBuilder Object(TObject result);
	TObject Build();

	bool MergeHasError(IResult<TIdentity> otherResult);

	bool MergeAllHasError(IResult<TIdentity> otherResult);


	bool HasError();

	object? GetData();

	TBuilder ClearAllSuccessMessages();

	TBuilder WithSuccess(ILogMessage<TIdentity> message);

	TBuilder WithWarn(ILogMessage<TIdentity> message);

	TBuilder WithError(IErrorMessage<TIdentity> message);

	TBuilder WithSuccess(MethodLogScope<TIdentity> scope, Action<LogMessageBuilder<TIdentity>>? logMessageConfigurator);

	TBuilder WithSuccess(ITraceInfo<TIdentity> traceInfo, Action<LogMessageBuilder<TIdentity>>? logMessageConfigurator);

	TBuilder WithWarn(MethodLogScope<TIdentity> scope, Action<LogMessageBuilder<TIdentity>>? logMessageConfigurator);

	TBuilder WithWarn(ITraceInfo<TIdentity> traceInfo, Action<LogMessageBuilder<TIdentity>>? logMessageConfigurator);

	TBuilder WithError(MethodLogScope<TIdentity> scope, Action<ErrorMessageBuilder<TIdentity>>? logMessageConfigurator);

	TBuilder WithError(ITraceInfo<TIdentity> traceInfo, Action<ErrorMessageBuilder<TIdentity>>? logMessageConfigurator);

	TBuilder ForAllSuccessMessages(Action<ILogMessage<TIdentity>> logMessageConfigurator);

	TBuilder ForAllWarningMessages(Action<ILogMessage<TIdentity>> logMessageConfigurator);

	TBuilder ForAllIErrorMessages(Action<ILogMessage<TIdentity>> errorMessageConfigurator);

	TBuilder ForAllMessages(Action<ILogMessage<TIdentity>> messageConfigurator);

	TBuilder Merge(IResult<TIdentity> otherResult);
}

public abstract class ResultBuilderBase<TBuilder, TObject, TIdentity> : IResultBuilder<TBuilder, TObject, TIdentity>
	where TBuilder : ResultBuilderBase<TBuilder, TObject, TIdentity>
	where TObject : IResult<TIdentity>
	where TIdentity : struct
{
	protected readonly TBuilder _builder;
	protected TObject _result;

	protected ResultBuilderBase(TObject result)
	{
		_result = result;
		_builder = (TBuilder)this;
	}

	public virtual TBuilder Object(TObject result)
	{
		_result = result ?? throw new ArgumentNullException(nameof(result));

		return _builder;
	}

	public TObject Build()
		=> _result;

	#region API

	public bool MergeHasError(IResult<TIdentity> otherResult)
	{
		if (otherResult != null && otherResult.HasError)
			_result.ErrorMessages.AddRange(otherResult.ErrorMessages);

		return _result.HasError;
	}

	public bool MergeAllHasError(IResult<TIdentity> otherResult)
	{
		if (otherResult != null)
		{
			if (otherResult.HasSuccessMessage)
				_result.SuccessMessages.AddRange(otherResult.SuccessMessages);

			if (otherResult.HasWarning)
				_result.WarningMessages.AddRange(otherResult.WarningMessages);

			if (otherResult.HasError)
				_result.ErrorMessages.AddRange(otherResult.ErrorMessages);
		}

		return _result.HasError;
	}

	public bool HasError()
	{
		return _result.HasError;
	}

	public virtual object? GetData()
	{
		return null;
	}

	public TBuilder ClearAllSuccessMessages()
	{
		_result.SuccessMessages.Clear();
		return _builder;
	}

	public TBuilder WithSuccess(ILogMessage<TIdentity> message)
	{
		_result.SuccessMessages.Add(message);
		return _builder;
	}

	public TBuilder WithWarn(ILogMessage<TIdentity> message)
	{
		_result.WarningMessages.Add(message);
		return _builder;
	}

	public TBuilder WithError(IErrorMessage<TIdentity> message)
	{
		_result.ErrorMessages.Add(message);
		return _builder;
	}

	public TBuilder WithSuccess(MethodLogScope<TIdentity> scope, Action<LogMessageBuilder<TIdentity>>? logMessageConfigurator)
		=> WithSuccess(scope?.TraceInfo!, logMessageConfigurator);

	public TBuilder WithSuccess(ITraceInfo<TIdentity> traceInfo, Action<LogMessageBuilder<TIdentity>>? logMessageConfigurator)
	{
		var logMessageBuilder =
			new LogMessageBuilder<TIdentity>(traceInfo)
				.LogLevel(LogLevel.Information);
		logMessageConfigurator?.Invoke(logMessageBuilder);
		_result.SuccessMessages.Add(logMessageBuilder.Build());
		return _builder;
	}

	public TBuilder WithWarn(MethodLogScope<TIdentity> scope, Action<LogMessageBuilder<TIdentity>>? logMessageConfigurator)
		=> WithWarn(scope?.TraceInfo!, logMessageConfigurator);

	public TBuilder WithWarn(ITraceInfo<TIdentity> traceInfo, Action<LogMessageBuilder<TIdentity>>? logMessageConfigurator)
	{
		var logMessageBuilder =
			new LogMessageBuilder<TIdentity>(traceInfo)
				.LogLevel(LogLevel.Warning);
		logMessageConfigurator?.Invoke(logMessageBuilder);
		_result.WarningMessages.Add(logMessageBuilder.Build());
		return _builder;
	}

	public TBuilder WithError(MethodLogScope<TIdentity> scope, Action<ErrorMessageBuilder<TIdentity>>? errorMessageConfigurator)
		=> WithError(scope?.TraceInfo!, errorMessageConfigurator);

	public TBuilder WithError(ITraceInfo<TIdentity> traceInfo, Action<ErrorMessageBuilder<TIdentity>>? errorMessageConfigurator)
	{
		var errorMessageBuilder =
			new ErrorMessageBuilder<TIdentity>(traceInfo)
				.LogLevel(LogLevel.Error);
		errorMessageConfigurator?.Invoke(errorMessageBuilder);
		_result.ErrorMessages.Add(errorMessageBuilder.Build());
		return _builder;
	}

	public TBuilder ForAllSuccessMessages(Action<ILogMessage<TIdentity>> logMessageConfigurator)
	{
		if (logMessageConfigurator == null)
			throw new ArgumentNullException(nameof(logMessageConfigurator));

		foreach (var successMessage in _result.SuccessMessages)
			logMessageConfigurator.Invoke(successMessage);

		return _builder;
	}

	public TBuilder ForAllWarningMessages(Action<ILogMessage<TIdentity>> logMessageConfigurator)
	{
		if (logMessageConfigurator == null)
			throw new ArgumentNullException(nameof(logMessageConfigurator));

		foreach (var warningMessage in _result.WarningMessages)
			logMessageConfigurator.Invoke(warningMessage);

		return _builder;
	}

	public TBuilder ForAllIErrorMessages(Action<ILogMessage<TIdentity>> errorMessageConfigurator)
	{
		if (errorMessageConfigurator == null)
			throw new ArgumentNullException(nameof(errorMessageConfigurator));

		foreach (var errorMessage in _result.ErrorMessages)
			errorMessageConfigurator.Invoke(errorMessage);

		return _builder;
	}

	public TBuilder ForAllMessages(Action<ILogMessage<TIdentity>> messageConfigurator)
		=> ForAllSuccessMessages(messageConfigurator)
			.ForAllWarningMessages(messageConfigurator)
			.ForAllIErrorMessages(messageConfigurator);

	public TBuilder Merge(IResult<TIdentity> otherResult)
	{
		if (otherResult != null)
		{
			if (otherResult.HasError)
				_result.ErrorMessages.AddRange(otherResult.ErrorMessages);
			if (otherResult.HasWarning)
				_result.WarningMessages.AddRange(otherResult.WarningMessages);
			if (otherResult.HasSuccessMessage)
				_result.SuccessMessages.AddRange(otherResult.SuccessMessages);
		}

		return _builder;
	}

	#endregion API
}

public class ResultBuilder<TIdentity> : ResultBuilderBase<ResultBuilder<TIdentity>, Result<TIdentity>, TIdentity>
	where TIdentity : struct
{
	public ResultBuilder()
		: this(new Result<TIdentity>())
	{
	}

	public ResultBuilder(Result<TIdentity> result)
		: base(result)
	{
	}

	public static implicit operator Result<TIdentity>?(ResultBuilder<TIdentity> builder)
	{
		if (builder == null)
			return null;

		return builder._result;
	}

	public static implicit operator ResultBuilder<TIdentity>?(Result<TIdentity> result)
	{
		if (result == null)
			return null;

		return new ResultBuilder<TIdentity>(result);
	}

	public static IResult<TIdentity> Empty()
		=> new ResultBuilder<TIdentity>().Build();
}

















public interface IResultBuilder<TBuilder, TData, TObject, TIdentity> : IResultBuilder<TBuilder, TObject, TIdentity>
	where TBuilder : IResultBuilder<TBuilder, TData, TObject, TIdentity>
	where TObject : IResult<TData, TIdentity>
	where TIdentity : struct
{
	TBuilder WithData(TData? data);

	TBuilder ClearData();

	bool MergeAllWithDataHasError(IResult<TData, TIdentity> otherResult);

	void MergeAllWithData(IResult<TData, TIdentity> otherResult);
}

public abstract class ResultBuilderBase<TBuilder, TData, TObject, TIdentity> : ResultBuilderBase<TBuilder, TObject, TIdentity>, IResultBuilder<TBuilder, TData, TObject, TIdentity>
	where TBuilder : ResultBuilderBase<TBuilder, TData, TObject, TIdentity>
	where TObject : IResult<TData, TIdentity>
	where TIdentity : struct
{
	protected ResultBuilderBase(TObject result)
		: base(result)
	{
	}

	public override object? GetData()
	{
		return _result.Data;
	}

	public TBuilder WithData(TData? data)
	{
		_result.Data = data;
		return _builder;
	}

	public TBuilder ClearData()
	{
		_result.ClearData();
		return _builder;
	}

	public bool MergeAllWithDataHasError(IResult<TData, TIdentity> otherResult)
	{
		if (otherResult != null)
		{
			if (otherResult.HasSuccessMessage)
				_result.SuccessMessages.AddRange(otherResult.SuccessMessages);

			if (otherResult.HasWarning)
				_result.WarningMessages.AddRange(otherResult.WarningMessages);

			if (otherResult.HasError)
				_result.ErrorMessages.AddRange(otherResult.ErrorMessages);

			_result.Data = otherResult.Data;
		}

		return _result.HasError;
	}

	public void MergeAllWithData(IResult<TData, TIdentity> otherResult)
		=> MergeAllWithDataHasError(otherResult);
}

public class ResultBuilder<TData, TIdentity> : ResultBuilderBase<ResultBuilder<TData, TIdentity>, TData, IResult<TData, TIdentity>, TIdentity>
	where TIdentity : struct
{
	public ResultBuilder()
		: this(new Result<TData, TIdentity>())
	{
	}

	public ResultBuilder(IResult<TData, TIdentity> result)
		: base(result)
	{
	}

	public static implicit operator Result<TData, TIdentity>?(ResultBuilder<TData, TIdentity> builder)
	{
		if (builder == null)
			return null;

		return builder._result as Result<TData, TIdentity>;
	}

	public static implicit operator ResultBuilder<TData, TIdentity>?(Result<TData, TIdentity> result)
	{
		if (result == null)
			return null;

		return new ResultBuilder<TData, TIdentity>(result);
	}

	public static implicit operator ResultBuilder<TIdentity>?(ResultBuilder<TData, TIdentity> builder)
	{
		if (builder == null)
			return null;

		return new ResultBuilder<TIdentity>((Result<TData, TIdentity>)builder._result);
	}

	public static IResult<TData, TIdentity> Empty()
		=> new ResultBuilder<TData, TIdentity>().Build();

	public static IResult<TData, TIdentity> FromResult(TData result)
		=> new ResultBuilder<TData, TIdentity>().WithData(result).Build();
}
