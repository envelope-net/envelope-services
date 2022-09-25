using Envelope.Logging;
using Envelope.Services.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

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
	ResultException? ToException(ILogger? logger = null, bool skipIfAlreadyLogged = true);

	void ThrowIfError();

	T? GetData<T>();

	bool TryGetData<T>([MaybeNullWhen(false)] out T data);
}

public interface IResult<TData> : IResult
{
	bool DataWasSet { get; }
	TData? Data { get; set; }

	void ClearData();
}