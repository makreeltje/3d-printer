using System.Text.RegularExpressions;
using System.Globalization;

namespace Services;

public class CuraGCodeParser : IGCodeParser
{
    public bool CanParse(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (line.Contains("Cura_SteamEngine"))
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
        double? density = null;
        double? diameter = null;

        // Cura specific regex
        var regexLength = new Regex(@";\s*Filament used: ([0-9.]+)m", RegexOptions.IgnoreCase);
        var regexTime = new Regex(@";\s*TIME:([0-9]+)", RegexOptions.IgnoreCase);
        var regexLayer = new Regex(@";\s*LAYER_COUNT:([0-9]+)", RegexOptions.IgnoreCase);
        var regexDensity = new Regex(@";\s*filament_density:? ?([0-9.]+)", RegexOptions.IgnoreCase);
        var regexDiameter = new Regex(@";\s*filament_diameter:? ?([0-9.]+)", RegexOptions.IgnoreCase);

        foreach (var line in lines)
        {
            var matchLength = regexLength.Match(line);
            if (matchLength.Success && double.TryParse(matchLength.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var meters))
                filamentUsedMm = meters * 1000;
            var matchTime = regexTime.Match(line);
            if (matchTime.Success && int.TryParse(matchTime.Groups[1].Value, out var seconds))
                estimatedTime = TimeSpan.FromSeconds(seconds);
            var matchLayer = regexLayer.Match(line);
            if (matchLayer.Success && int.TryParse(matchLayer.Groups[1].Value, out var layers))
                layerCount = layers;
            var matchDensity = regexDensity.Match(line);
            if (matchDensity.Success && double.TryParse(matchDensity.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dens))
                density = dens;
            var matchDiameter = regexDiameter.Match(line);
            if (matchDiameter.Success && double.TryParse(matchDiameter.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dia))
                diameter = dia;
        }

        // Fallback for grams calculation
        // PLA defaults: density = 1.24 g/cm³, diameter = 1.75 mm
        double usedDensity = density ?? 1.24;
        double usedDiameter = diameter ?? 1.75;
        if (filamentUsedMm > 0)
        {
            double r = usedDiameter / 2.0;
            double volume = Math.PI * r * r * filamentUsedMm / 1000.0; // mm^3 to cm^3
            filamentUsedGrams = volume * usedDensity; // density in g/cm^3
        }

        return new GCodeParseResult
        {
            Slicer = "UltiMaker Cura",
            FilamentUsedGrams = Math.Round(filamentUsedGrams, 2),
            FilamentUsedMm = Math.Round(filamentUsedMm, 2),
            EstimatedPrintTime = estimatedTime,
            LayerCount = layerCount
        };
    }
}
