using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RZ.Foundation.Json;

public static class JsonSerializerOptionsExtension
{
    public static JsonSerializerOptions UseRzConverters(this JsonSerializerOptions opts) {
        if (!opts.Converters.Any(c => c is JsonStringEnumConverter))
            opts.Converters.Add(new JsonStringEnumConverter());
        opts.Converters.Add(new OptionJsonConverter());
        opts.Converters.Add(new SeqJsonConverter());
        opts.Converters.Add(new SetJsonConverter());
        opts.Converters.Add(new MapJsonConverter());
        opts.Converters.Add(new OutcomeConverter());
        return opts;
    }

    public static JsonSerializerOptions UseCommonApiResponseSettings(this JsonSerializerOptions opts) {
        opts.WriteIndented = false;
        opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opts.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        opts.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        return opts;
    }

    /// <summary>
    /// Recommendation for JSON serialization settings.
    /// </summary>
    /// <param name="opts">a JSON option to be set</param>
    /// <returns>The same, modified, JSON object</returns>
    public static JsonSerializerOptions UseRzRecommendedSettings(this JsonSerializerOptions opts)
        => opts.UseCommonApiResponseSettings().UseRzConverters();

    /// <summary>
    /// GraphQL style has enum literals in UPPER SNAKE_CASE.
    /// </summary>
    /// <param name="opts"></param>
    /// <returns></returns>
    public static JsonSerializerOptions UseRzGraphStyleSettings(this JsonSerializerOptions opts) {
        opts.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
        return opts.UseCommonApiResponseSettings();
    }
}