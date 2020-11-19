using System;
using ADepIn;
using Valve.Newtonsoft.Json;

namespace Deli
{
    public class OptionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Option<>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var generics = objectType.GetGenericArguments();

            if (reader.TokenType == JsonToken.Null)
            {
                var optionNone = typeof(Option).GetMethod(nameof(Option.None)).MakeGenericMethod(generics);

                return optionNone.Invoke(null, new object[0]);
            }

            var value = serializer.Deserialize(reader, generics[0]);
            var optionSome = typeof(Option).GetMethod(nameof(Option.Some)).MakeGenericMethod(generics);

            return optionSome.Invoke(null, new[] {value});
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var matchSomeMethod = value.GetType().GetMethod(nameof(Option<object>.MatchSome));

            var match = new object[] {null};
            if ((bool) matchSomeMethod.Invoke(value, match))
                serializer.Serialize(writer, match[0]);
            else
                writer.WriteNull();
        }
    }
}