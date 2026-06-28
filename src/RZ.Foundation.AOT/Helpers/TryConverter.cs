using System.Globalization;

namespace RZ.Foundation.Helpers;

[PublicAPI]
public static class TryConvert
{
    public static Option<bool>           ToBoolean(string value)                          => bool.TryParse(value, out var v) ? v : None;
    public static Option<byte>           ToByte(string value)                             => byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : None;
    public static Option<short>          ToInt16(string value)                            => short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : None;
    public static Option<int>            ToInt32(string value)                            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : None;
    public static Option<long>           ToInt64(string value)                            => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : None;
    public static Option<float>          ToSingle(string value)                           => float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v) ? v : None;
    public static Option<double>         ToDouble(string value)                           => double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var v) ? v : None;
    public static Option<decimal>        ToDecimal(string value)                          => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : None;
    public static Option<DateTime>       ToDateTime(string value)                         => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var v) ? v : None;
    public static Option<DateTimeOffset> ToDateTimeOffset(string value)                   => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var v) ? v : None;
    public static Option<Guid>           ToGuid(string value)                             => Guid.TryParse(value, out var v) ? v : None;
    public static Option<TimeSpan>       ToTimeSpan(string value)                         => TimeSpan.TryParse(value, out var v) ? v : None;
    public static Option<Uri>            ToUri(string value)                              => Uri.TryCreate(value, UriKind.Absolute, out var v) ? v : None;
    public static Option<TEnum>          ToEnum<TEnum>(string value) where TEnum : struct => Enum.TryParse(value, out TEnum v) ? v : None;
}