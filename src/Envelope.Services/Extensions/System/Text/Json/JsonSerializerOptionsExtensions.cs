#if NET6_0_OR_GREATER
using Envelope.Services.Serializers.JsonConverters;
using System.Text.Json;

namespace Envelope.Extensions;

public static class JsonSerializerOptionsExtensions
{
	public static JsonSerializerOptions AddServiceReadConverters(this JsonSerializerOptions options)
	{
		if (options == null)
			throw new ArgumentNullException(nameof(options));

		JsonConvertersConfig.AddServiceReadConverters(options.Converters);
		return options;
	}
}
#endif
