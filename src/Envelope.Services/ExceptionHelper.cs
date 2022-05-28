using Envelope.Extensions;
using Envelope.Services.Exceptions;

namespace Envelope.Services;

public static class ExceptionHelper_TIdentity
{
	/// <summary>
	/// Returns null if no Error message found
	/// </summary>
	public static ResultException? ToException(IResult result)
	{
		if (result == null || !result.HasError)
			return null;

		var exception = Logging.ExceptionHelper.ToException(result.ErrorMessages[0], msg => new ResultException(msg));

		for (int i = 1; i < result.ErrorMessages.Count; i++)
			exception.AppendLogMessage(result.ErrorMessages[i]);

		return exception;
	}
}
