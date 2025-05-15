using System.Text.RegularExpressions;
using System.Globalization;

namespace Services;

public class CrealityPrintGCodeParser : IGCodeParser
{
    public bool CanParse(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (line.Contains("Creality_Print"))
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
        var regexWeight = new Regex(@";\s*filament used \[g\] ?= ?([0-9.]+)", RegexOptions.IgnoreCase);
        var regexLength = new Regex(@";\s*filament used \[mm\] ?= ?([0-9.]+)", RegexOptions.IgnoreCase);
        var regexTime = new Regex(@";\s*estimated printing time.*= ?(?:(\d+)d)? ?(?:(\d+)h)? ?(?:(\d+)m)? ?(?:(\d+)s)?", RegexOptions.IgnoreCase);
        var regexLayer = new Regex(@";\s*total layer number:? ?([0-9]+)", RegexOptions.IgnoreCase);

        foreach (var line in lines)
        {
            var matchWeight = regexWeight.Match(line);
            if (matchWeight.Success && double.TryParse(matchWeight.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var grams))
                filamentUsedGrams = grams;
            var matchLength = regexLength.Match(line);
            if (matchLength.Success && double.TryParse(matchLength.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var mm))
                filamentUsedMm = mm;
            var matchTime = regexTime.Match(line);
            if (matchTime.Success)
            {
                int d = 0, h = 0, m = 0, s = 0;
                if (matchTime.Groups[1].Success && !string.IsNullOrEmpty(matchTime.Groups[1].Value)) int.TryParse(matchTime.Groups[1].Value, out d);
                if (matchTime.Groups[2].Success && !string.IsNullOrEmpty(matchTime.Groups[2].Value)) int.TryParse(matchTime.Groups[2].Value, out h);
                if (matchTime.Groups[3].Success && !string.IsNullOrEmpty(matchTime.Groups[3].Value)) int.TryParse(matchTime.Groups[3].Value, out m);
                if (matchTime.Groups[4].Success && !string.IsNullOrEmpty(matchTime.Groups[4].Value)) int.TryParse(matchTime.Groups[4].Value, out s);
                estimatedTime = new TimeSpan(d, h, m, s);
            }
            var matchLayer = regexLayer.Match(line);
            if (matchLayer.Success && int.TryParse(matchLayer.Groups[1].Value, out var layers))
                layerCount = layers;
        }

        return new GCodeParseResult
        {
            Slicer = "Creality Print",
            FilamentUsedGrams = Math.Round(filamentUsedGrams, 2),
            FilamentUsedMm = Math.Round(filamentUsedMm, 2),
            EstimatedPrintTime = estimatedTime,
            LayerCount = layerCount
        };
    }
}
