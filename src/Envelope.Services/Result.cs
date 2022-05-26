using Envelope.Services.Exceptions;
using Envelope.Logging;

namespace Envelope.Services;

public class Result<TIdentity> : IResult<TIdentity>
	where TIdentity : struct
{
	public List<ILogMessage<TIdentity>> SuccessMessages { get; }

	public List<ILogMessage<TIdentity>> WarningMessages { get; }

	public List<IErrorMessage<TIdentity>> ErrorMessages { get; }

	public bool HasSuccessMessage => 0 < SuccessMessages.Count;

	public bool HasWarning => 0 < WarningMessages.Count;

	public bool HasError => 0 < ErrorMessages.Count;

	public bool HasAnyMessage => HasSuccessMessage || HasWarning || HasError;

	public long? AffectedEntities { get; set; }

	internal Result()
	{
		SuccessMessages = new List<ILogMessage<TIdentity>>();
		WarningMessages = new List<ILogMessage<TIdentity>>();
		ErrorMessages = new List<IErrorMessage<TIdentity>>();
	}

	public ResultException<TIdentity>? ToException()
		=> ExceptionHelper.ToException(this);

	public void ThrowIfError()
	{
		var exception = ToException();

		if (exception != null)
			throw exception;
	}
}

public class Result<TData, TIdentity> : Result<TIdentity>, IResult<TData, TIdentity>, IResult<TIdentity>
	where TIdentity : struct
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
