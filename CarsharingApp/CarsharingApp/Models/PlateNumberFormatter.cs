using System.Text;
using System.Text.RegularExpressions;

namespace CarsharingApp.Models;

public static class PlateNumberFormatter
{
    // Older seed data was read as Windows-1251 after being written in DOS CP866.
    // Repair only a reversible conversion that produces the expected plate shape.
    public static string Format(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Не указан";
        var raw = value.Trim();
        if (IsPlate(raw.ToUpperInvariant())) return raw.ToUpperInvariant();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        try
        {
            var legacy = Encoding.GetEncoding(1251, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            var repaired = Encoding.GetEncoding(866).GetString(legacy.GetBytes(raw));
            if (IsPlate(repaired.ToUpperInvariant())) return repaired.ToUpperInvariant();
        }
        catch (EncoderFallbackException) { }
        return raw;
    }

    private static bool IsPlate(string value) => Regex.IsMatch(value, @"^[А-ЯA-Z][0-9]{3}[А-ЯA-Z]{2}[0-9]{2,3}$");
}
