using System.Text.Json;
using RZ.Foundation.Json;

namespace RZ.Foundation;

public partial class Prelude
{
    public static readonly JsonSerializerOptions RzRecommendedJsonOptions = new JsonSerializerOptions().UseRzRecommendedSettings();
}