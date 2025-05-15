using System.Text.RegularExpressions;
using System.Globalization;

namespace Services;

public class OrcaSlicerGCodeParser : IGCodeParser
{
    public bool CanParse(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (line.Contains("OrcaSlicer"))
                return true;
        }
        return false;
    }

    public GCodeParseResult Parse(IEnumerable<string> lines)
    {
        double filamentUsedMm = 0;
        double filamentUsedGrams = 0;
        TimeSpan? estimatedTime = null;
        int? layerCount = null;
        var regexWeight = new Regex(@";\s*total filament used \[g\] ?= ?([0-9.]+)", RegexOptions.IgnoreCase);
        var regexLength = new Regex(@";\s*filament used \[mm\] ?= ?([0-9.]+), ?([0-9.]+)", RegexOptions.IgnoreCase);
        var regexTime = new Regex(@";\s*estimated printing time.*= ?([0-9]+)h? ?([0-9]+)m? ?([0-9]+)s?", RegexOptions.IgnoreCase);
        var regexLayer = new Regex(@";\s*total layers count ?= ?([0-9]+)", RegexOptions.IgnoreCase);

        foreach (var line in lines)
        {
            var matchWeight = regexWeight.Match(line);
            if (matchWeight.Success && double.TryParse(matchWeight.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var grams))
                filamentUsedGrams = grams;
            var matchLength = regexLength.Match(line);
            // Use the second group if present (second number after comma)
            if (matchLength.Success)
            {
                // Try to get the second value, fallback to the first if not present
                string mmStr = matchLength.Groups.Count > 2 && !string.IsNullOrWhiteSpace(matchLength.Groups[2].Value)
                    ? matchLength.Groups[2].Value
                    : matchLength.Groups[1].Value;
                if (double.TryParse(mmStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var mm))
                    filamentUsedMm = mm;
            }
            var matchTime = regexTime.Match(line);
            if (matchTime.Success)
            {
                int m = 0, s = 0;
                if (matchTime.Groups[1].Success) int.TryParse(matchTime.Groups[1].Value.TrimEnd('m'), out m);
                if (matchTime.Groups[2].Success) int.TryParse(matchTime.Groups[2].Value.TrimEnd('s'), out s);
                estimatedTime = new TimeSpan(0, m, s);
            }
            var matchLayer = regexLayer.Match(line);
            if (matchLayer.Success && int.TryParse(matchLayer.Groups[1].Value, out var layers))
                layerCount = layers;
        }

        return new GCodeParseResult
        {
            Slicer = "OrcaSlicer",
            FilamentUsedGrams = Math.Round(filamentUsedGrams, 2),
            FilamentUsedMm = Math.Round(filamentUsedMm, 2),
            EstimatedPrintTime = estimatedTime,
            LayerCount = layerCount
        };
    }
}
