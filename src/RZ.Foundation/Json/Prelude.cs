using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using RZ.Foundation.Json;
using RZ.Foundation.Types;

namespace RZ.Foundation;

public static class Prelude
{
    public static readonly JsonSerializerOptions RzRecommendedJsonOptions = new JsonSerializerOptions().UseRzRecommendedSettings();

    #region JSON serialization helpers

    [PublicAPI]
    public static Outcome<string> JsonSerialize<T>(in T data, JsonSerializerOptions? options = null) where T: notnull {
        try{
            return JsonSerializer.Serialize(data, options ?? RzRecommendedJsonOptions);
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    [PublicAPI]
    public static Outcome<T> JsonDeserialize<T>(string jsonText, JsonSerializerOptions? options = null) where T : notnull {
        try{
            return JsonSerializer.Deserialize<T>(jsonText, options ?? RzRecommendedJsonOptions) is { } result ? result : ErrorInfo.NotFound;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    [PublicAPI]
    public static Outcome<T> JsonDeserialize<T>(JsonNode json, JsonSerializerOptions? options = null) where T : notnull {
        try{
            return json.Deserialize<T>(options ?? RzRecommendedJsonOptions) is { } result ? result : ErrorInfo.NotFound;
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    #endregion
}