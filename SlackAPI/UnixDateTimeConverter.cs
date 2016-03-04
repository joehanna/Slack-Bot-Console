using System;

using Newtonsoft.Json;

namespace Pook.SlackAPI
{
	public class UnixDateTimeConverter : JsonConverter
	{
		private static readonly DateTime epoc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTime);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.Integer)
				throw new InvalidCastException($"Unexpected token parsing date. Expected Integer, got {reader.TokenType} = {reader.Value}");

			var value = (long)reader.Value;
			var res = new DateTime(epoc.Ticks + (value * 10000000), DateTimeKind.Utc);
			return res;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue((((DateTime)value).ToUniversalTime() - epoc).TotalSeconds);
		}
	}
}