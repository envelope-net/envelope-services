using Envelope.Services.Exceptions;
using Envelope.Logging;

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

	public ResultException? ToException()
		=> ExceptionHelper_TIdentity.ToException(this);

	public void ThrowIfError()
	{
		var exception = ToException();

		if (exception != null)
			throw exception;
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
}
