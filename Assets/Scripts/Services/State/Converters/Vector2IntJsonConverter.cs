using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Services.State.Converters {
	sealed class Vector2IntJsonConverter : JsonConverter {
		public override bool CanConvert(Type objectType) => objectType == typeof(Vector2Int);

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
			if (value == null) {
				throw new ArgumentNullException(nameof(value));
			}
			var vector = (Vector2Int)value;
			var obj = new JObject {
				{ "x", new JValue(vector.x) },
				{ "y", new JValue(vector.y) }
			};
			obj.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
			var vector = new Vector2Int();
			var isXProperty = false;
			var isYProperty = false;
			while ( reader.Read() ) {
				switch ( reader.TokenType ) {
					case JsonToken.PropertyName: {
						var value = reader.Value;
						if (value == null) {
							throw new InvalidOperationException("Expected property name cannot be null");
						}
						var propertyName = (string)value;
						isXProperty = propertyName == "x";
						isYProperty = propertyName == "y";
						break;
					}
					case JsonToken.Integer: {
						var value = reader.Value;
						if (value == null) {
							throw new InvalidOperationException("Expected integer value cannot be null");
						}
						var intValue = (int)(long)value;
						if ( isXProperty ) {
							vector.x = intValue;
						}
						if ( isYProperty ) {
							vector.y = intValue;
						}
						break;
					}
					case JsonToken.EndObject: return vector;
				}
			}
			throw new InvalidOperationException();
		}

	}
}
