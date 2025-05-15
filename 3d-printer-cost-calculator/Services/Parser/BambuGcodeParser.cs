using System.Globalization;
using System.Text.RegularExpressions;
using _3d_printer_cost_calculator.Models;

namespace Services.Parser;

public class BambuGcodeParser : IGcodeParser
{
    public bool CanParse(string[] gcodeLines)
    {
        return gcodeLines.Any(line => line.Contains("BambuStudio"));
    }

    public ParsedGcode Parse(string[] gcodeLines)
    {
        var parsed = new ParsedGcode
        {
            SlicerName = "Bambu Studio"
        };

        foreach (var line in gcodeLines)
        {
            if (line.StartsWith("; total filament length [mm] : "))
                parsed.FilamentUsedMm = double.Parse(line.Split(':').Last().Trim(), CultureInfo.InvariantCulture);
            if (line.StartsWith("; total filament weight [g] : "))
                parsed.FilamentUsedGrams = double.Parse(line.Split(':').Last().Trim(), CultureInfo.InvariantCulture);
            if (line.StartsWith("; estimated printing time (normal mode) = "))
                parsed.EstimatedPrintTime = ParseTime(line.Split('=').Last().Trim());
            if (line.StartsWith("; total layer number: "))
                parsed.LayerCount = int.Parse(line.Split(':').Last().Trim());
            if (line.StartsWith("; layer_height = "))
                parsed.LayerHeight = double.Parse(line.Split('=').Last().Trim(), CultureInfo.InvariantCulture);
            if (line.StartsWith("START_PRINT"))
            {
                var match = Regex.Match(line, @"EXTRUDER_TEMP=(\d+)\s+BED_TEMP=(\d+)");
                if (match.Success)
                {
                    parsed.NozzleTemperature = int.Parse(match.Groups[1].Value);
                    parsed.BedTemperature = int.Parse(match.Groups[2].Value);
                }
            }
            if (line.StartsWith("; BambuStudio "))
                parsed.SlicerVersion = line.Split(' ').Last().Trim();
        }
        return parsed;
    }
    
    private TimeSpan ParseTime(string timeStr)
    {
        int days = 0, hours = 0, minutes = 0, seconds = 0;

        var matches = Regex.Matches(timeStr, @"(\d+)([dhms])");
        foreach (Match match in matches)
        {
            int value = int.Parse(match.Groups[1].Value);
            switch (match.Groups[2].Value)
            {
                case "d": days = value; break;
                case "h": hours = value; break;
                case "m": minutes = value; break;
                case "s": seconds = value; break;
            }
        }

        return new TimeSpan(days, hours, minutes, seconds);
    }
    
}