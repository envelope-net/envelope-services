#if NET6_0_OR_GREATER
using Envelope.Extensions;
using System.Text.Json.Serialization;

namespace Envelope.Services.Serializers.JsonConverters;

public static class JsonConvertersConfig
{
	public static void AddServiceReadConverters(IList<JsonConverter> converters)
	{
		if (converters == null)
			throw new ArgumentNullException(nameof(converters));

		Envelope.Logging.Serializers.JsonConverters.JsonConvertersConfig.AddLoggingReadConverters(converters);
		converters.AddUniqueItem(new ResultJsonConverter());
		converters.AddUniqueItem(new ResultWithDataJsonConverterFactory());
	}
}
#endif
