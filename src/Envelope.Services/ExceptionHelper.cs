using Envelope.Extensions;
using Envelope.Logging.Extensions;
using Envelope.Services.Exceptions;
using Microsoft.Extensions.Logging;

namespace Envelope.Services;

public static class ExceptionHelper
{
	/// <summary>
	/// Returns null if no Error message found
	/// </summary>
	public static ResultException? ToException(IResult result, ILogger? logger = null, bool skipIfAlreadyLogged = true)
	{
		if (result == null || !result.HasError)
			return null;

		if (logger != null)
		{
			try
			{
				logger.LogResultErrorMessages(result, skipIfAlreadyLogged);
			}
			catch { }
		}

		var exception = Logging.ExceptionHelper.ToException(result.ErrorMessages[0], msg => new ResultException(msg));

		for (int i = 1; i < result.ErrorMessages.Count; i++)
			exception.AppendLogMessage(result.ErrorMessages[i]);

		return exception;
	}
}
