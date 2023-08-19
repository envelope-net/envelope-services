using Envelope.Services.Exceptions;
using Envelope.Logging;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Envelope.Services;

public class ResultDto : IResult
{
	public List<ILogMessage> SuccessMessages { get; set; }

	public List<ILogMessage> WarningMessages { get; set; }

	public List<IErrorMessage> ErrorMessages { get; set; }

	public bool HasSuccessMessage => 0 < SuccessMessages.Count;

	public bool HasWarning => 0 < WarningMessages.Count;

	public bool HasError => 0 < ErrorMessages.Count;

#if NETSTANDARD2_0 || NETSTANDARD2_1
	[Newtonsoft.Json.JsonIgnore]
#elif NET6_0_OR_GREATER
	[System.Text.Json.Serialization.JsonIgnore]
#endif
	public bool HasTransactionRollbackError => ErrorMessages.Where(x => !x.DisableTransactionRollback).Any();

	public bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

	public long? AffectedEntities { get; set; }

	public ResultDto()
	{
		SuccessMessages = new List<ILogMessage>();
		WarningMessages = new List<ILogMessage>();
		ErrorMessages = new List<IErrorMessage>();
	}

	public ResultException? ToException(ILogger? logger = null, bool skipIfAlreadyLogged = true)
		=> ExceptionHelper.ToException(this);

	public void ThrowIfError()
	{
		var exception = ToException();

		if (exception != null)
			throw exception;
	}

	public virtual object? GetData()
		=> default;

	public T? GetData<T>()
		=> GetDataInternal<T>();

	protected internal virtual T? GetDataInternal<T>()
		=> default;

	public bool TryGetData<T>([MaybeNullWhen(false)] out T data)
		=> TryGetDataInternal(out data);

	protected internal virtual bool TryGetDataInternal<T>([MaybeNullWhen(false)] out T data)
	{
		data = default;
		return false;
	}
}

public class ResultDto<TData> : ResultDto, IResult<TData>, IResult
{
	public bool DataWasSet { get; set; }

	public TData? Data { get; set; }

	public ResultDto()
		: base()
	{
	}

	public void ClearData()
	{
		Data = default;
		DataWasSet = false;
	}

	public override object? GetData()
		=> Data;

	protected internal override T GetDataInternal<T>()
	{
		if (Data is T data)
			return data;

		return default!;
	}

	protected internal override bool TryGetDataInternal<T>([MaybeNullWhen(false)] out T data)
	{
		if (Data is T d)
		{
			data = d;
			return true;
		}

		data = default!;
		return false;
	}
}
