using System.Globalization;

namespace MsfLang.Core.Runtime;

public static class ValueFormatter
{
    public static string Format(MsfValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            MsfNumber n => FormatNumber(n.Value),
            MsfString s => s.Value,
            MsfBoolean b => b.Value ? "true" : "false",
            MsfNil => "nil",
            MsfList list => FormatList(list),
            MsfMap map => FormatMap(map),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value.GetType(), "Unsupported value type")
        };
    }

    private static string FormatNumber(double value)
    {
        if (double.IsNegative(value) && value == 0d)
        {
            return "0";
        }

        // Round-trip representation, invariant culture. "G17" for double gives round-trippable shortest-ish form.
        var text = value.ToString("G17", CultureInfo.InvariantCulture);
        // Normalize possible "+E" forms if any (G17 usually uses E+/- in some cases?) - ensure lowercase 'e' and no leading zeros in exponent.
        if (text.Contains('E', StringComparison.Ordinal))
        {
            var parts = text.Split('E');
            if (parts.Length == 2 && int.TryParse(parts[1], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var exp))
            {
                text = parts[0] + "e" + exp.ToString(CultureInfo.InvariantCulture);
            }
        }

        return text;
    }

    private static string FormatList(MsfList list)
        => "[" + string.Join(", ", list.Items.Select(FormatElement)) + "]";

    private static string FormatMap(MsfMap map)
        => "{" + string.Join(", ", map.Items.Select(kv => FormatElement(new MsfString(kv.Key)) + ": " + FormatElement(kv.Value))) + "}";

    private static string FormatElement(MsfValue value) => value switch
    {
        MsfString s => "\"" + s.Value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\r", "\\r") + "\"",
        MsfList list => FormatList(list),
        _ => Format(value)
    };
}
