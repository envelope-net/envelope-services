#if NET6_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Envelope.Services.Serializers.JsonConverters;

public class ResultWithDataJsonConverterFactory : JsonConverterFactory
{
	private static readonly Type _resultType = typeof(IResult<>);

	public override bool CanConvert(Type typeToConvert)
	{
		if (!typeToConvert.IsGenericType)
			return false;

		if (typeToConvert.GetGenericTypeDefinition() != _resultType)
			return false;

		return true;
	}

	public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		Type dataType = typeToConvert.GetGenericArguments()[0];

		var converter = (JsonConverter)Activator.CreateInstance(typeof(ResultWithDataJsonConverter<>).MakeGenericType(dataType))!;

		return converter;
	}
}
#endif
