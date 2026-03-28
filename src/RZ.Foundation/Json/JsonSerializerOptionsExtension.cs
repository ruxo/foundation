using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using RZ.Foundation.Types;

namespace RZ.Foundation.Json;

public static class JsonSerializerOptionsExtension
{
    /// <param name="opts">a JSON option to be set</param>
    extension(JsonSerializerOptions opts)
    {
        [PublicAPI]
        public JsonSerializerOptions UseRzConverters() {
            opts.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            if (!opts.Converters.Any(c => c is JsonStringEnumConverter))
                opts.Converters.Add(new JsonStringEnumConverter());
            opts.Converters.Add(new OptionJsonConverter());
            opts.Converters.Add(new SeqJsonConverter());
            opts.Converters.Add(new SetJsonConverter());
            opts.Converters.Add(new MapJsonConverter());
            opts.Converters.Add(new OutcomeConverter());
            return opts;
        }

        [PublicAPI]
        public JsonSerializerOptions UseCommonApiResponseSettings() {
            opts.WriteIndented = false;
            opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opts.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            opts.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            return opts;
        }

        /// <summary>
        /// Recommendation for JSON serialization settings.
        /// </summary>
        /// <returns>The same, modified, JSON object</returns>
        [PublicAPI]
        public JsonSerializerOptions UseRzRecommendedSettings()
            => opts.UseCommonApiResponseSettings().UseRzConverters();

        /// <summary>
        /// GraphQL style has enum literals in UPPER SNAKE_CASE.
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public JsonSerializerOptions UseRzGraphStyleSettings() {
            opts.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
            return opts.UseCommonApiResponseSettings();
        }
    }

    public static Outcome<T> TryDeserialize<T>(this JsonNode node, JsonSerializerOptions? options = null) {
        try{
            return node.Deserialize<T>(options ?? RzRecommendedJsonOptions) is {} v? v : ErrorInfo.NotFound;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }
}