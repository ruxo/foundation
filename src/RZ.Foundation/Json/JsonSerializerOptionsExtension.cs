using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RZ.Foundation.Json;

public static class JsonSerializerOptionsExtension
{
    public static JsonSerializerOptions UseRzConverters(this JsonSerializerOptions opts) {
        opts.Converters.Add(new JsonStringEnumConverter());
        opts.Converters.Add(new OptionJsonConverter());
        opts.Converters.Add(new SeqJsonConverter());
        opts.Converters.Add(new SetJsonConverter());
        opts.Converters.Add(new MapJsonConverter());
        return opts;
    }

    public static JsonSerializerOptions UseRzRecommendedSettings(this JsonSerializerOptions opts) {
        opts.WriteIndented = false;
        opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opts.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        opts.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        return opts.UseRzConverters();
    }
}