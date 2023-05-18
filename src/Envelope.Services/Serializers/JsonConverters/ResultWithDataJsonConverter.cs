#if NET6_0_OR_GREATER
using Envelope.Extensions;
using Envelope.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Envelope.Services.Serializers.JsonConverters;

public class ResultWithDataJsonConverter<TData> : JsonConverter<IResult<TData>>
{
	private static readonly Type _logMessageList = typeof(List<ILogMessage>);
	private static readonly Type _errorMessageList = typeof(List<IErrorMessage>);

	public override void Write(Utf8JsonWriter writer, IResult<TData> value, JsonSerializerOptions options)
	{
		throw new NotImplementedException("Read only converter");
	}

	public override IResult<TData>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
			var stringComparison = options.PropertyNameCaseInsensitive
				? StringComparison.OrdinalIgnoreCase
				: StringComparison.Ordinal;

			var result = new Result<TData>
			{
			};

			TData? data = default;
			var dataWasSet = false;
			while (reader.Read())
			{
				if (reader.TokenType == JsonTokenType.EndObject)
				{
					if (dataWasSet)
						result.Data = data;

					return result;
				}

				if (reader.TokenType == JsonTokenType.PropertyName)
				{
					var propertyName = reader.GetString();
					reader.Read();
					switch (propertyName)
					{
						case var name when string.Equals(name, nameof(IResult<TData>.SuccessMessages), stringComparison):
							if (reader.TokenType != JsonTokenType.Null)
							{
								var successMessages = ((JsonConverter<List<ILogMessage>>)options.GetConverter(_logMessageList)).Read(ref reader, _logMessageList, options);
								if (0 < successMessages?.Count)
									foreach (var successMessage in successMessages)
										result.SuccessMessages.Add(successMessage);
							}
							break;
						case var name when string.Equals(name, nameof(IResult<TData>.WarningMessages), stringComparison):
							if (reader.TokenType != JsonTokenType.Null)
							{
								var warningMessages = ((JsonConverter<List<ILogMessage>>)options.GetConverter(_logMessageList)).Read(ref reader, _logMessageList, options);
								if (0 < warningMessages?.Count)
									foreach (var warningMessage in warningMessages)
										result.WarningMessages.Add(warningMessage);
							}
							break;
						case var name when string.Equals(name, nameof(IResult<TData>.ErrorMessages), stringComparison):
							if (reader.TokenType != JsonTokenType.Null)
							{
								var errorMessages = ((JsonConverter<List<IErrorMessage>>)options.GetConverter(_errorMessageList)).Read(ref reader, _errorMessageList, options);
								if (0 < errorMessages?.Count)
									foreach (var errorMessage in errorMessages)
										result.ErrorMessages.Add(errorMessage);
							}
							break;
						case var name when string.Equals(name, nameof(IResult<TData>.Data), stringComparison):
							if (reader.TokenType != JsonTokenType.Null)
							{
								var dataType = typeof(TData).GetUnderlyingNullableType();
								data = ((JsonConverter<TData>)options.GetConverter(dataType)).Read(ref reader, dataType, options);
							}

							break;
						case var name when string.Equals(name, nameof(IResult<TData>.DataWasSet), stringComparison):
							dataWasSet = reader.TokenType != JsonTokenType.Null && reader.GetBoolean();
							break;
					}
				}
			}

			return default;
		}
	}
}
#endif
