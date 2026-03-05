using System.Globalization;

namespace LawllitFinance.Web;

public static class StringHelpers
{
    public static string ToTitleCase(string value)
        => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim().ToLower());
}
