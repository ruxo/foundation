using System.Text;

namespace RZ.Foundation.Helpers;

public static class SnakeCase
{
    public static string ToSnakeCase(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        var stringBuilder = new StringBuilder();
        var wasPrevUpper = false;
        var wasPrevDigit = false;
        var wasPrevUnderscore = false;
        var addUnderscore = false;

        for (var i = 0; i < str.Length; i++)
        {
            var c = str[i];
            if (c == '_' || c == ' ') {
                wasPrevDigit = wasPrevUpper = wasPrevUnderscore = false;
                addUnderscore = true;
            }
            else if (char.IsUpper(c))
            {
                if (stringBuilder.Length != 0 && !wasPrevUpper && !wasPrevDigit && !wasPrevUnderscore)
                {
                    stringBuilder.Append('_');
                    addUnderscore = false;
                }
                stringBuilder.Append(char.ToLowerInvariant(c));
                wasPrevUpper = true;
                wasPrevDigit = false;
                wasPrevUnderscore = false;
            }
            else if (char.IsDigit(c))
            {
                if (stringBuilder.Length != 0 && !wasPrevDigit && !wasPrevUnderscore)
                {
                    stringBuilder.Append('_');
                    addUnderscore = false;
                }
                stringBuilder.Append(c);
                wasPrevUpper = false;
                wasPrevDigit = true;
                wasPrevUnderscore = false;
            }
            else {
                if (addUnderscore)
                    stringBuilder.Append('_');
                stringBuilder.Append(c);
                wasPrevUpper = wasPrevDigit = wasPrevUnderscore = addUnderscore = false;
            }
        }

        return stringBuilder.ToString();
    } 
}