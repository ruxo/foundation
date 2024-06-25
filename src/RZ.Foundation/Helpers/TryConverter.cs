namespace RZ.Foundation.Helpers
{
    public static class TryConvert
    {
        public static Option<bool> ToBoolean(string value) => bool.TryParse(value, out var v) ? v : None;
        public static Option<byte> ToByte(string value) => byte.TryParse(value, out var v) ? v : None;
        public static Option<short> ToInt16(string value) => short.TryParse(value, out var v) ? v : None;
        public static Option<int> ToInt32(string value) => int.TryParse(value, out var v) ? v : None;
        public static Option<long> ToInt64(string value) => long.TryParse(value, out var v) ? v : None;
    }
}