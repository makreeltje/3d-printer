using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using _3d_printer_cost_calculator.Models;
using Microsoft.Extensions.Logging;

namespace _3d_printer_cost_calculator.Services
{
    public interface IGCodeParserService
    {
        Task<GCodeFile> ParseGCodeFileAsync(Stream fileStream, string fileName);
        GCodeFile ParseGCodeFile(string[] lines, string fileName);
        CostCalculation CalculateCost(GCodeFile gCodeFile, decimal filamentPricePerKg, decimal electricityPricePerKwh, decimal printerCost, decimal hourlyLaborRate, decimal printerPowerConsumption);
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
                _logger.LogInformation($"Parsing GCODE file: {fileName}");

                using var reader = new StreamReader(fileStream);
                var content = await reader.ReadToEndAsync();
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                return ParseGCodeFile(lines, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error parsing GCODE file: {fileName}");
                throw;
            }
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
            double filamentDiameter = 1.75; // Default value
            double filamentDensity = 1.24; // Default PLA density (g/cm³)
            double layerHeight = 0.2; // Default value
            int layerCount = 0;
            double nozzleTemp = 0;
            double bedTemp = 0;
            TimeSpan estimatedTime = TimeSpan.Zero;
            bool hasSupport = false;
            double infillPercentage = 0;
            string slicerSoftware = "Unknown";

            // Pattern matching for common slicer comments
            var estimatedTimePattern = new Regex(@"estimated printing time.*?(\d+h)?.*?(\d+m)?.*?(\d+s)?", RegexOptions.IgnoreCase);
            var filamentUsedPattern = new Regex(@"filament\s+used.*?(\d+\.?\d*).*?(mm|cm|m)", RegexOptions.IgnoreCase);
            var filamentWeightPattern = new Regex(@"filament\s+used.*?(\d+\.?\d*).*?(g)", RegexOptions.IgnoreCase);
            var layerHeightPattern = new Regex(@"layer.*?height.*?(\d+\.?\d*)", RegexOptions.IgnoreCase);
            var filamentDiameterPattern = new Regex(@"filament.*?diameter.*?(\d+\.?\d*)", RegexOptions.IgnoreCase);
            var slicerPattern = new Regex(@"generated.*?(Cura|PrusaSlicer|Simplify3D|slic3r|IdeaMaker)", RegexOptions.IgnoreCase);
            var nozzleTempPattern = new Regex(@"M109 S(\d+\.?\d*)|M104 S(\d+\.?\d*)", RegexOptions.IgnoreCase);
            var bedTempPattern = new Regex(@"M190 S(\d+\.?\d*)|M140 S(\d+\.?\d*)", RegexOptions.IgnoreCase);
            var infillPattern = new Regex(@"infill.*?(\d+\.?\d*)%", RegexOptions.IgnoreCase);
            var supportPattern = new Regex(@"support.*?(yes|enabled|true)", RegexOptions.IgnoreCase);

            // Track layer changes
            var layerChangePattern = new Regex(@";LAYER:(\d+)", RegexOptions.IgnoreCase);

            // Track extrusion moves to calculate filament used
            var extrusionPattern = new Regex(@"G1.*?E(\d+\.?\d*)", RegexOptions.IgnoreCase);
            double lastE = 0;

            foreach (var line in lines)
            {
                // Check for slicer software
                var slicerMatch = slicerPattern.Match(line);
                if (slicerMatch.Success && slicerMatch.Groups[1].Success)
                {
                    slicerSoftware = slicerMatch.Groups[1].Value;
                }

                // Extract layer height
                var layerHeightMatch = layerHeightPattern.Match(line);
                if (layerHeightMatch.Success && layerHeightMatch.Groups[1].Success)
                {
                    layerHeight = double.Parse(layerHeightMatch.Groups[1].Value);
                }

                // Extract filament diameter
                var filamentDiameterMatch = filamentDiameterPattern.Match(line);
                if (filamentDiameterMatch.Success && filamentDiameterMatch.Groups[1].Success)
                {
                    filamentDiameter = double.Parse(filamentDiameterMatch.Groups[1].Value);
                }

                // Parse estimated printing time from comments
                var timeMatch = estimatedTimePattern.Match(line);
                if (timeMatch.Success)
                {
                    int hours = 0, minutes = 0, seconds = 0;

                    if (timeMatch.Groups[1].Success)
                    {
                        hours = int.Parse(Regex.Match(timeMatch.Groups[1].Value, @"\d+").Value);
                    }

                    if (timeMatch.Groups[2].Success)
                    {
                        minutes = int.Parse(Regex.Match(timeMatch.Groups[2].Value, @"\d+").Value);
                    }

                    if (timeMatch.Groups[3].Success)
                    {
                        seconds = int.Parse(Regex.Match(timeMatch.Groups[3].Value, @"\d+").Value);
                    }

                    estimatedTime = new TimeSpan(hours, minutes, seconds);
                }

                // Check for direct filament length information
                var filamentLengthMatch = filamentUsedPattern.Match(line);
                if (filamentLengthMatch.Success && filamentLengthMatch.Groups[1].Success)
                {
                    double value = double.Parse(filamentLengthMatch.Groups[1].Value);
                    string unit = filamentLengthMatch.Groups[2].Value.ToLower();

                    // Convert to mm
                    if (unit == "m")
                        filamentLength = value * 1000;
                    else if (unit == "cm")
                        filamentLength = value * 10;
                    else // mm
                        filamentLength = value;
                }

                // Check for nozzle temperature
                var nozzleTempMatch = nozzleTempPattern.Match(line);
                if (nozzleTempMatch.Success)
                {
                    var tempValue = nozzleTempMatch.Groups[1].Success 
                        ? nozzleTempMatch.Groups[1].Value 
                        : nozzleTempMatch.Groups[2].Value;
                    nozzleTemp = double.Parse(tempValue);
                }

                // Check for bed temperature
                var bedTempMatch = bedTempPattern.Match(line);
                if (bedTempMatch.Success)
                {
                    bedTemp = double.Parse(bedTempMatch.Groups[1].Value);
                }

                // Check for infill percentage
                var infillMatch = infillPattern.Match(line);
                if (infillMatch.Success && infillMatch.Groups[1].Success)
                {
                    infillPercentage = double.Parse(infillMatch.Groups[1].Value);
                }

                // Check if supports are used
                var supportMatch = supportPattern.Match(line);
                if (supportMatch.Success)
                {
                    hasSupport = true;
                }

                // Track layer changes
                var layerMatch = layerChangePattern.Match(line);
                if (layerMatch.Success && layerMatch.Groups[1].Success)
                {
                    int layer = int.Parse(layerMatch.Groups[1].Value);
                    layerCount = Math.Max(layerCount, layer + 1); // +1 because layers are 0-indexed
                }

                // Track extrusion moves to calculate filament used if not provided directly
                if (filamentLength == 0)
                {
                    var extrusionMatch = extrusionPattern.Match(line);
                    if (extrusionMatch.Success && extrusionMatch.Groups[1].Success)
                    {
                        double e = double.Parse(extrusionMatch.Groups[1].Value);
                        if (e > lastE) // Only count positive extrusion (not retractions)
                        {
                            filamentLength += (e - lastE);
                        }
                        lastE = e;
                    }
                }
            }

            // Calculate filament weight if not directly found in comments
            double filamentWeightGrams = 0;
            var filamentWeightMatch = filamentWeightPattern.Match(string.Join("\n", lines));
            if (filamentWeightMatch.Success && filamentWeightMatch.Groups[1].Success)
            {
                filamentWeightGrams = double.Parse(filamentWeightMatch.Groups[1].Value);
            }
            else if (filamentLength > 0)
            {
                // Calculate weight from length: volume = π * (diameter/2)² * length
                double radius = filamentDiameter / 2; // in mm
                double volume = Math.PI * radius * radius * filamentLength; // in mm³
                filamentWeightGrams = (volume / 1000) * filamentDensity; // convert to cm³ and multiply by density
            }

            // Set the parsed values to the GCodeFile object
            gCodeFile.FilamentUsageLength = filamentLength;
            gCodeFile.FilamentUsageWeight = filamentWeightGrams;
            gCodeFile.EstimatedPrintTime = estimatedTime;
            gCodeFile.LayerCount = layerCount;
            gCodeFile.LayerHeight = layerHeight;
            gCodeFile.NozzleTemperature = nozzleTemp;
            gCodeFile.BedTemperature = bedTemp;
            gCodeFile.FilamentDiameter = filamentDiameter;
            gCodeFile.HasSupport = hasSupport;
            gCodeFile.InfillPercentage = infillPercentage;
            gCodeFile.SlicerSoftware = slicerSoftware;

            _logger.LogInformation($"Successfully parsed GCODE file: {fileName}");
            return gCodeFile;
        }

        public CostCalculation CalculateCost(GCodeFile gCodeFile, decimal filamentPricePerKg, decimal electricityPricePerKwh, decimal printerCost, decimal hourlyLaborRate, decimal printerPowerConsumption)
        {
            try
            {
                _logger.LogInformation($"Calculating costs for GCODE file: {gCodeFile.Filename}");

                // Convert print time to hours
                decimal printTimeHours = (decimal)gCodeFile.EstimatedPrintTime.TotalHours;

                // Calculate material cost
                decimal filamentWeightKg = (decimal)gCodeFile.FilamentUsageWeight / 1000; // Convert grams to kg
                decimal materialCost = filamentWeightKg * filamentPricePerKg;

                // Calculate electricity cost
                decimal electricityCost = printTimeHours * (printerPowerConsumption / 1000) * electricityPricePerKwh;

                // Calculate machine depreciation cost (simplified)
                decimal expectedLifespan = 2000; // Assume 2000 hours of printer life
                decimal depreciationCost = (printerCost / expectedLifespan) * printTimeHours;

                // Calculate labor cost (assume 15 min setup time)
                decimal setupTimeHours = 0.25m;
                decimal laborCost = setupTimeHours * hourlyLaborRate;

                // Calculate total cost
                decimal totalCost = materialCost + electricityCost + depreciationCost + laborCost;

                // Create and return the cost calculation
                var costCalculation = new CostCalculation
                {
                    MaterialCost = materialCost,
                    ElectricityCost = electricityCost,
                    DepreciationCost = depreciationCost,
                    LaborCost = laborCost,
                    TotalCost = totalCost,
                    Currency = "USD", // Default currency
                    GCodeFile = gCodeFile
                };

                _logger.LogInformation($"Cost calculation completed for: {gCodeFile.Filename}, total cost: {totalCost:F2}");
                return costCalculation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating costs for GCODE file: {gCodeFile.Filename}");
                throw;
            }
        }
    }
}
