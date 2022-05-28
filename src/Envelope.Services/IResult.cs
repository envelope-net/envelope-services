using Envelope.Logging;
using Envelope.Services.Exceptions;

namespace Envelope.Services;

public interface IResult
{
	List<ILogMessage> SuccessMessages { get; }

	List<ILogMessage> WarningMessages { get; }

	List<IErrorMessage> ErrorMessages { get; }

	bool HasSuccessMessage { get; }

	bool HasWarning { get; }

	bool HasError { get; }

	bool HasAnyMessage { get; }

	/// <summary>
	/// Returns null if no Error message found
	/// </summary>
	ResultException? ToException();

	void ThrowIfError();
}

public interface IResult<TData> : IResult
{
	bool DataWasSet { get; }
	TData? Data { get; set; }

	void ClearData();
}