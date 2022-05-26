using System.Runtime.Serialization;

namespace Envelope.Services.Exceptions;

public class ResultException<TIdentity> : Exception
	where TIdentity : struct
{
	public ResultException()
		: base()
	{ }

	public ResultException(string? message)
		: base(message)
	{ }

	public ResultException(string? message, Exception? innerException)
		: base(message, innerException)
	{ }

	protected ResultException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
