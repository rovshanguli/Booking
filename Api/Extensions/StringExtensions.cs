using System.Globalization;
using System.Text.RegularExpressions;

namespace Api.Extensions
{
    public static class StringExtensions
    {
        public static DateOnly? ParseToDate(this string? s) =>
            DateOnly.TryParseExact(s, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;
    }
}