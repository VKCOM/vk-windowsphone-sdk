using System;
using Newtonsoft.Json;

namespace VK.WindowsPhone.SDK.Json
{
	public class JsonUnixTimeConverter
		: JsonConverter
	{
		private static DateTime _baseUnixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public override bool CanWrite => false;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (!CanConvert(objectType))
			{
				throw new JsonSerializationException("Unexpected target type");
			}

			long unixTime;

			if (reader.TokenType == JsonToken.String)
			{
				var stringValue = reader.Value?.ToString();
				if (stringValue == null)
				{
					throw new JsonSerializationException("Unexpected token value an integer is expected");
				}

				if (!long.TryParse(stringValue, out unixTime))
				{
					throw new JsonSerializationException("Unexpected token value an integer is expected");
				}
			}
			else if (reader.TokenType == JsonToken.Integer)
			{
				unixTime = Convert.ToInt64(reader.Value);
			}
			else
			{
				throw new JsonSerializationException("Unexpected token value an integer is expected");
			}

			if (objectType == typeof(DateTimeOffset))
			{
				return new DateTimeOffset(_baseUnixTime).AddSeconds(unixTime);
			}

			return _baseUnixTime.AddSeconds(unixTime);
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset);
		}
	}
}
