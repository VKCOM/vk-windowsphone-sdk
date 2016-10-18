using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace VK.WindowsPhone.SDK.Json
{
	public class JsonBoolConverter
		: JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if(!(value is bool))
			{
				throw new Exception("valule is not a bool"); 
			}

			writer.WriteValue((bool)value ? "1" : "0");
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if(reader.TokenType == JsonToken.Boolean)
			{
				return (bool)reader.Value;
			}

			if(reader.TokenType == JsonToken.Integer)
			{
				return Convert.ToInt64(reader.Value) == 1;
			}

			Debug.WriteLine("Unable to convert {0}:{1} to bool", reader.TokenType, reader.Value);

			return false;
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(bool);
		}
	}
}