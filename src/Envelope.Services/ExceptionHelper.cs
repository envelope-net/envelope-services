using Envelope.Extensions;
using Envelope.Services.Exceptions;

namespace Envelope.Services;

public static class ExceptionHelper
{
	/// <summary>
	/// Returns null if no Error message found
	/// </summary>
	public static ResultException<TIdentity>? ToException<TIdentity>(IResult<TIdentity> result)
		where TIdentity : struct
	{
		if (result == null || !result.HasError)
			return null;

		var exception = Logging.ExceptionHelper.ToException(result.ErrorMessages[0], msg => new ResultException<TIdentity>(msg));

		for (int i = 1; i < result.ErrorMessages.Count; i++)
			exception.AppendLogMessage(result.ErrorMessages[i]);

		return exception;
	}
}
