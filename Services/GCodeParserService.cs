using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using _3d_printer_cost_calculator.Models;

namespace _3d_printer_cost_calculator.Services
{
    public interface IGCodeParserService
    {
        Task<GCodeFile> ParseGCodeFileAsync(Stream fileStream, string fileName);
    }

    public class GCodeParserService : IGCodeParserService
    {
        private readonly ILogger<GCodeParserService> _logger;

        public GCodeParserService(ILogger<GCodeParserService> logger)
        {
            _logger = logger;
        }

        public async Task<GCodeFile> ParseGCodeFileAsync(Stream fileStream, string fileName)
        {
            try
            {
                _logger.LogInformation("Parsing GCODE file: {FileName}", fileName);

                using var reader = new StreamReader(fileStream);
                var content = await reader.ReadToEndAsync();
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                return ParseGCodeFile(lines, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing GCODE file: {FileName}", fileName);
            }
            return null!;
        }

        public GCodeFile ParseGCodeFile(string[] lines, string fileName)
        {
            var gCodeFile = new GCodeFile
            {
                Filename = fileName,
                ParsedDate = DateTime.Now
            };

            // Initialize variables to track during parsing
            double filamentLength = 0;
            double filamentDiameter = 0; // Default value
            // Default PLA density (g/cm³)
            double layerHeight = 0; // Default value
            int layerCount = 0;
            double nozzleTemp = 0;
            double bedTemp = 0;
            TimeSpan estimatedTime = TimeSpan.Zero;
            bool hasSupport = false;
            double infillPercentage = 0;
            string slicerSoftware = "Unknown";
            string filamentType = "Unknown";
            string? thumbnailBase64 = null;

            // Pattern matching for common slicer comments
            var estimatedTimePattern = new Regex(@"estimated printing time.*?=\s*(([\d]+)h)?\s*(([\d]+)m)?\s*(([\d]+)s)?", RegexOptions.IgnoreCase);
            var filamentUsedPattern = new Regex(@"filament.*?used.*?\[mm\].*?=\s*([\d\.\,\s]+)", RegexOptions.IgnoreCase);
            var filamentWeightPattern = new Regex(@"filament.*?used.*?\[g\].*?=\s*([\d\.\,\s]+)", RegexOptions.IgnoreCase);
            var layerHeightPattern = new Regex(@"; layer.*?height.*?(\d+\.?\d*)", RegexOptions.IgnoreCase);
            var filamentDiameterPattern = new Regex(@"filament.*?diameter.*?(\d+\.?\d*)", RegexOptions.IgnoreCase);
            var slicerPattern = new Regex(@"generated.*?(Cura|PrusaSlicer|Simplify3D|slic3r|IdeaMaker|OrcaSlicer|Creality_Print)", RegexOptions.IgnoreCase);
            // Updated pattern to better match temperature commands in GCODE
            var nozzleTempPattern = new Regex(@"EXTRUDER_TEMP=(\d+\.?\d*)", RegexOptions.IgnoreCase);
            var bedTempPattern = new Regex(@"BED_TEMP=(\d+\.?\d*)", RegexOptions.IgnoreCase);
            var infillPattern = new Regex(@"infill.*?density.*?(\d+\.?\d*)%", RegexOptions.IgnoreCase);
            var supportPattern = new Regex(@"enable.*?support.*?(\d+\.?\d*)", RegexOptions.IgnoreCase);
            var totalLayersPattern = new Regex(@"total.*?layers.*?count.*?(\d+)", RegexOptions.IgnoreCase);
            var filamentTypePattern = new Regex(@"; filament.*?type.*?([A-Z]+)", RegexOptions.IgnoreCase);
            // Matches thumbnail start/end delimiters in GCODE
            var thumbnailStartPattern = new Regex(@"; thumbnail begin (\d+)x(\d+) (\d+)", RegexOptions.IgnoreCase);
            var thumbnailEndPattern = new Regex(@"; thumbnail end", RegexOptions.IgnoreCase);


            // Extract thumbnail data
            bool inThumbnail = false;
            int thumbnailWidth = 0;
            int thumbnailHeight = 0;
            int thumbnailDataLines = 0;
            StringBuilder thumbnailData = new StringBuilder();

            foreach (var line in lines)
            {
                // Check for thumbnail start
                if (!inThumbnail)
                {
                    var thumbnailStartMatch = thumbnailStartPattern.Match(line);
                    if (thumbnailStartMatch.Success)
                    {
                        inThumbnail = true;
                        thumbnailData.Clear();
                        if (int.TryParse(thumbnailStartMatch.Groups[1].Value, out thumbnailWidth) &&
                            int.TryParse(thumbnailStartMatch.Groups[2].Value, out thumbnailHeight) &&
                            int.TryParse(thumbnailStartMatch.Groups[3].Value, out thumbnailDataLines))
                        {
                            // Found start of thumbnail, prepare to collect data
                            continue;
                        }
                    }
                }
                else
                {
                    // Check for thumbnail end
                    if (thumbnailEndPattern.Match(line).Success)
                    {
                        inThumbnail = false;
                        
                        // Convert collected base64 data
                        if (thumbnailData.Length > 0)
                        {
                            thumbnailBase64 = thumbnailData.ToString();
                        }
                    }
                    
                    // Inside thumbnail section - collect the data
                    if (line.StartsWith(';'))
                    {
                        // Remove the leading semicolon and any whitespace
                        string cleanedLine = line.TrimStart(';', ' ', '\t');
                        thumbnailData.Append(cleanedLine);
                        continue;
                    }
                }
                
                // Check for slicer software
                var slicerMatch = slicerPattern.Match(line);
                if (slicerMatch.Success && slicerMatch.Groups[1].Success)
                {
                    slicerSoftware = slicerMatch.Groups[1].Value;
                }

                // Check for filament type
                var filamentTypeMatch = filamentTypePattern.Match(line);
                if (filamentTypeMatch.Success && filamentTypeMatch.Groups[1].Success)
                {
                    filamentType = filamentTypeMatch.Groups[1].Value;
                }

                // Extract layer height
                var layerHeightMatch = layerHeightPattern.Match(line);
                if (layerHeightMatch.Success && double.TryParse(layerHeightMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedHeight))
                {
                    layerHeight = parsedHeight;
                }
                
                var filamentDiameterMatch = filamentDiameterPattern.Match(line);
                if (filamentDiameterMatch.Success && filamentDiameterMatch.Groups[1].Success)
                {
                    _ = double.TryParse(filamentDiameterMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out filamentDiameter);
                }

                // Parse estimated printing time from comments
                var timeMatch = estimatedTimePattern.Match(line);
                if (timeMatch.Success)
                {
                    int hours = 0, minutes = 0, seconds = 0;

                    // Group 2, 4, 6 contain the actual numbers from the new regex pattern
                    if (timeMatch.Groups.Count > 2 && timeMatch.Groups[2].Success)
                    {
                        _ = int.TryParse(timeMatch.Groups[2].Value, out hours);
                    }

                    if (timeMatch.Groups.Count > 4 && timeMatch.Groups[4].Success)
                    {
                        _ = int.TryParse(timeMatch.Groups[4].Value, out minutes);
                    }

                    if (timeMatch.Groups.Count > 6 && timeMatch.Groups[6].Success)
                    {
                        _ = int.TryParse(timeMatch.Groups[6].Value, out seconds);
                    }
                    estimatedTime = new TimeSpan(hours, minutes, seconds);
                }

                // Check for direct filament length information
                var filamentLengthMatch = filamentUsedPattern.Match(line);
                if (filamentLengthMatch.Success && filamentLengthMatch.Groups.Count > 1)
                {
                    double highestValue = ExtractHighestValueFromCommaSeparatedList(filamentLengthMatch.Groups[1].Value);
                    if (highestValue > 0)
                    {
                        filamentLength = highestValue;
                    }
                }
                // Check for nozzle temperature
                var nozzleTempMatch = nozzleTempPattern.Match(line);
                if (nozzleTempMatch.Success && nozzleTempMatch.Groups.Count > 1)
                {
                    nozzleTemp = ExtractTemperatureFromMatch(nozzleTempMatch);
                }
                
                // Check for bed temperature
                var bedTempMatch = bedTempPattern.Match(line);
                if (bedTempMatch.Success && bedTempMatch.Groups.Count > 1)
                {
                    bedTemp = ExtractTemperatureFromMatch(bedTempMatch);
                }
                var infillMatch = infillPattern.Match(line);
                if (infillMatch.Success && infillMatch.Groups[1].Success)
                {
                    _ = double.TryParse(infillMatch.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out infillPercentage);
                }

                // Check if supports are used
                var supportMatch = supportPattern.Match(line);
                if (supportMatch.Success)
                {
                    hasSupport = true;
                }

                // Check for total layers count information
                var totalLayersMatch = totalLayersPattern.Match(line);
                if (totalLayersMatch.Success && totalLayersMatch.Groups[1].Success && int.TryParse(totalLayersMatch.Groups[1].Value, out int totalLayers))
                {
                    layerCount = totalLayers;
                }
            }

            // Calculate filament weight if not directly found in comments
            double filamentWeightGrams = 0;
            
            // First try to parse filament weight directly from comments
            var filamentWeightMatch = filamentWeightPattern.Match(string.Join("\n", lines));
            if (filamentWeightMatch.Success && filamentWeightMatch.Groups.Count > 1)
            {
                filamentWeightGrams = ExtractHighestValueFromCommaSeparatedList(filamentWeightMatch.Groups[1].Value);
            }

            // Set the parsed values to the GCodeFile object
            gCodeFile.FilamentUsageLength = filamentLength; // Store actual length in mm
            gCodeFile.FilamentUsageWeight = filamentWeightGrams; // Store actual weight in grams
            gCodeFile.EstimatedPrintTime = estimatedTime;
            gCodeFile.LayerCount = layerCount;
            gCodeFile.LayerHeight = layerHeight; // Already correctly parsed
            gCodeFile.NozzleTemperature = nozzleTemp;
            gCodeFile.BedTemperature = bedTemp;
            gCodeFile.FilamentDiameter = filamentDiameter;
            gCodeFile.HasSupport = hasSupport;
            gCodeFile.InfillPercentage = infillPercentage;
            gCodeFile.SlicerSoftware = slicerSoftware;
            gCodeFile.FilamentType = filamentType;
            gCodeFile.ThumbnailBase64 = thumbnailBase64 ?? string.Empty;

            _logger.LogInformation("Successfully parsed GCODE file: {FileName}", fileName);
            return gCodeFile;
        }

        /// <summary>
        /// Extracts temperature value from a regex match
        /// </summary>
        /// <param name="match">The regex match containing temperature information</param>
        /// <returns>The extracted temperature value or 0 if parsing fails</returns>
        private static double ExtractTemperatureFromMatch(Match match)
        {
            // Check each capture group in order
            for (int i = 1; i < match.Groups.Count; i++)
            {
                if (match.Groups[i].Success)
                {
                    if (double.TryParse(match.Groups[i].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedTemp))
                    {
                        return parsedTemp;
                    }
                    break;
                }
            }
            
            return 0;
        }

        /// <summary>
        /// Extracts the highest value from a comma-separated list
        /// </summary>
        /// <param name="commaSeparatedValues">String containing comma-separated values</param>
        /// <returns>The highest non-zero value found, or 0 if no valid values</returns>
        private static double ExtractHighestValueFromCommaSeparatedList(string commaSeparatedValues)
        {
            if (string.IsNullOrEmpty(commaSeparatedValues))
            {
                return 0;
            }

            double highestValue = 0;
            string[] values = commaSeparatedValues.Split(',');
                
            foreach (string val in values)
            {
                if (double.TryParse(val.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedValue) && parsedValue > highestValue)
                {
                    highestValue = parsedValue;
                }
            }
            
            return highestValue;
        }
    }
}
