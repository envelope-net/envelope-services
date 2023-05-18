#if NET6_0_OR_GREATER
using Envelope.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Envelope.Services.Serializers.JsonConverters;

public class ResultJsonConverter : JsonConverter<IResult>
{
	private static readonly Type _logMessageList = typeof(List<ILogMessage>);
	private static readonly Type _errorMessageList = typeof(List<IErrorMessage>);

	public override void Write(Utf8JsonWriter writer, IResult value, JsonSerializerOptions options)
	{
		throw new NotImplementedException("Read only converter");
	}

	public override IResult? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType == JsonTokenType.Null)
		{
			return null;
		}

		if (reader.TokenType != JsonTokenType.StartObject)
		{
			throw new JsonException();
		}
		else
		{
			var result = new Result
			{
			};

			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					return result;
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString();
					reader.Read();
					switch (propertyName)
					{
						case nameof(IResult.SuccessMessages):
							if (reader.TokenType != JsonTokenType.Null)
							{
								var successMessages = ((JsonConverter<List<ILogMessage>>)options.GetConverter(_logMessageList)).Read(ref reader, _logMessageList, options);
								if (0 < successMessages?.Count)
									foreach (var successMessage in successMessages)
										result.SuccessMessages.Add(successMessage);
							}
							break;
						case nameof(IResult.WarningMessages):
							if (reader.TokenType != JsonTokenType.Null)
							{
								var warningMessages = ((JsonConverter<List<ILogMessage>>)options.GetConverter(_logMessageList)).Read(ref reader, _logMessageList, options);
								if (0 < warningMessages?.Count)
									foreach (var warningMessage in warningMessages)
										result.WarningMessages.Add(warningMessage);
							}
							break;
						case nameof(IResult.ErrorMessages):
							if (reader.TokenType != JsonTokenType.Null)
							{
								var errorMessages = ((JsonConverter<List<IErrorMessage>>)options.GetConverter(_errorMessageList)).Read(ref reader, _errorMessageList, options);
								if (0 < errorMessages?.Count)
									foreach (var errorMessage in errorMessages)
										result.ErrorMessages.Add(errorMessage);
							}
							break;
					}
				}
			}

			return default;
		}
	}
}
#endif
