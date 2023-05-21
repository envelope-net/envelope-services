using Envelope.Services.Exceptions;
using Envelope.Logging;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Envelope.Services;

public class Result : IResult
{
	public List<ILogMessage> SuccessMessages { get; }

	public List<ILogMessage> WarningMessages { get; }

	public List<IErrorMessage> ErrorMessages { get; }

	public bool HasSuccessMessage => 0 < SuccessMessages.Count;

	public bool HasWarning => 0 < WarningMessages.Count;

	public bool HasError => 0 < ErrorMessages.Count;

	public bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

	public long? AffectedEntities { get; set; }

	internal Result()
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

public class Result<TData> : Result, IResult<TData>, IResult
{
	public bool DataWasSet { get; private set; }

	private TData? _data;
	public TData? Data
	{
		get
		{
			return _data;
		}
		set
		{
			_data = value;
			DataWasSet = true;
		}
	}

	internal Result()
		: base()
	{
	}

	public void ClearData()
	{
		_data = default;
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
