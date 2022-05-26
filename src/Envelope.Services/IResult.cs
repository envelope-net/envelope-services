using Envelope.Logging;
using Envelope.Services.Exceptions;

namespace Envelope.Services;

public interface IResult<TIdentity>
	where TIdentity : struct
{
	List<ILogMessage<TIdentity>> SuccessMessages { get; }

	List<ILogMessage<TIdentity>> WarningMessages { get; }

	List<IErrorMessage<TIdentity>> ErrorMessages { get; }

	bool HasSuccessMessage { get; }

	bool HasWarning { get; }

	bool HasError { get; }

	bool HasAnyMessage { get; }

	/// <summary>
	/// Returns null if no Error message found
	/// </summary>
	ResultException<TIdentity>? ToException();

	void ThrowIfError();
}

public interface IResult<TData, TIdentity> : IResult<TIdentity>
	where TIdentity : struct
{
	bool DataWasSet { get; }
	TData? Data { get; set; }

	void ClearData();
}