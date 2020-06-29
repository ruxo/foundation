using System;
using Newtonsoft.Json;
using RZ.Foundation.Types;

namespace RZ.Foundation.NewtonsoftJson
{
    public sealed class TimeRangeJsonConverter : JsonConverter
    {
        public string TimeText = string.Empty;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            var range = (TimeRange?) value;
            writer.WriteValue(range!.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) =>
            TimeRange.Parse((string)reader.Value!);

        public override bool CanConvert(Type objectType) => objectType == typeof(TimeRange);
    }
}