using Envelope.Logging;
using Envelope.Logging.Extensions;
using Microsoft.Extensions.Logging;

namespace Envelope.Services.Extensions;

public static class LoggerExtensions
{
	public static void LogResultErrorMessages<TIdentity>(this ILogger logger, IResult<TIdentity> result, bool skipIfAlreadyLogged)
		where TIdentity : struct
	{
		if (logger == null)
			throw new ArgumentNullException(nameof(logger));

		if (result == null)
			throw new ArgumentNullException(nameof(result));

		foreach (var errorMessage in result.ErrorMessages)
		{
			if (errorMessage.LogLevel == LogLevel.Error)
				logger.LogErrorMessage(errorMessage, skipIfAlreadyLogged);
			else if (errorMessage.LogLevel == LogLevel.Critical)
				logger.LogCriticalMessage(errorMessage, skipIfAlreadyLogged);
			else
				throw new NotSupportedException($"{nameof(errorMessage.LogLevel)} = {errorMessage.LogLevel}");
		}
	}

	public static void LogResultAllMessages<TIdentity>(this ILogger logger, IResult<TIdentity> result, bool skipIfAlreadyLogged)
		where TIdentity : struct
	{
		if (logger == null)
			throw new ArgumentNullException(nameof(logger));

		if (result == null)
			throw new ArgumentNullException(nameof(result));

		var messages = new List<ILogMessage<TIdentity>>(result.ErrorMessages);
		messages.AddRange(result.WarningMessages);
		messages.AddRange(result.SuccessMessages);

		messages  = messages.OrderBy(x => x.CreatedUtc).ToList();
		foreach (var message in messages)
		{
			switch (message.LogLevel)
			{
				case LogLevel.Trace:
					logger.LogTraceMessage(message, skipIfAlreadyLogged);
					break;
				case LogLevel.Debug:
					logger.LogDebugMessage(message, skipIfAlreadyLogged);
					break;
				case LogLevel.Information:
					logger.LogInformationMessage(message, skipIfAlreadyLogged);
					break;
				case LogLevel.Warning:
					logger.LogWarningMessage(message, skipIfAlreadyLogged);
					break;
				case LogLevel.Error:
					logger.LogErrorMessage((message as IErrorMessage<TIdentity>)!, skipIfAlreadyLogged);
					break;
				case LogLevel.Critical:
					logger.LogCriticalMessage((message as IErrorMessage<TIdentity>)!, skipIfAlreadyLogged);
					break;
				default:
					throw new NotSupportedException($"{nameof(message.LogLevel)} = {message.LogLevel}");
			}
		}
	}

	public static void LogResultErrorMessages(this ILogger logger, IResult<Guid> result, bool skipIfAlreadyLogged)
	{
		if (logger == null)
			throw new ArgumentNullException(nameof(logger));

		if (result == null)
			throw new ArgumentNullException(nameof(result));

		foreach (var errorMessage in result.ErrorMessages)
		{
			if (errorMessage.LogLevel == LogLevel.Error)
				logger.LogErrorMessage(errorMessage, skipIfAlreadyLogged);
			else if (errorMessage.LogLevel == LogLevel.Critical)
				logger.LogCriticalMessage(errorMessage, skipIfAlreadyLogged);
			else
				throw new NotSupportedException($"{nameof(errorMessage.LogLevel)} = {errorMessage.LogLevel}");
		}
	}

	public static void LogResultAllMessages(this ILogger logger, IResult<Guid> result, bool skipIfAlreadyLogged)
	{
		if (logger == null)
			throw new ArgumentNullException(nameof(logger));

		if (result == null)
			throw new ArgumentNullException(nameof(result));

		var messages = new List<ILogMessage<Guid>>(result.ErrorMessages);
		messages.AddRange(result.WarningMessages);
		messages.AddRange(result.SuccessMessages);

		messages = messages.OrderBy(x => x.CreatedUtc).ToList();
		foreach (var message in messages)
		{
			switch (message.LogLevel)
			{
				case LogLevel.Trace:
					logger.LogTraceMessage(message, skipIfAlreadyLogged);
					break;
				case LogLevel.Debug:
					logger.LogDebugMessage(message, skipIfAlreadyLogged);
					break;
				case LogLevel.Information:
					logger.LogInformationMessage(message, skipIfAlreadyLogged);
					break;
				case LogLevel.Warning:
					logger.LogWarningMessage(message, skipIfAlreadyLogged);
					break;
				case LogLevel.Error:
					logger.LogErrorMessage((message as IErrorMessage<Guid>)!, skipIfAlreadyLogged);
					break;
				case LogLevel.Critical:
					logger.LogCriticalMessage((message as IErrorMessage<Guid>)!, skipIfAlreadyLogged);
					break;
				default:
					throw new NotSupportedException($"{nameof(message.LogLevel)} = {message.LogLevel}");
			}
		}
	}
}
