using System;

namespace _3d_printer_cost_calculator.Models
{
    public class GCodeFile
    {
        /// <summary>
        /// The name of the GCODE file
        /// </summary>
        public string? Filename { get; set; } = string.Empty;

        /// <summary>
        /// Length of filament used in millimeters
        /// </summary>
        public double FilamentUsageLength { get; set; }

        /// <summary>
        /// Filament type (PLA, ABS, PETG, etc.)
        /// </summary>
        public string? FilamentType { get; set; } = string.Empty;

        /// <summary>
        /// Weight of filament used in grams
        /// </summary>
        public double FilamentUsageWeight { get; set; }

        /// <summary>
        /// Estimated time to complete the print
        /// </summary>
        public TimeSpan EstimatedPrintTime { get; set; }

        /// <summary>
        /// Total number of layers in the print
        /// </summary>
        public int LayerCount { get; set; }

        /// <summary>
        /// Height of each layer in millimeters
        /// </summary>
        public double LayerHeight { get; set; }

        /// <summary>
        /// Extruder temperature in Celsius
        /// </summary>
        public double NozzleTemperature { get; set; }

        /// <summary>
        /// Bed temperature in Celsius
        /// </summary>
        public double BedTemperature { get; set; }

        /// <summary>
        /// Filament diameter in millimeters
        /// </summary>
        public double FilamentDiameter { get; set; }

        /// <summary>
        /// Infill percentage (0-100)
        /// </summary>
        public double InfillPercentage { get; set; }

        /// <summary>
        /// Whether the print uses support structures
        /// </summary>
        public bool HasSupport { get; set; }

        /// <summary>
        /// The date when the file was parsed
        /// </summary>
        public DateTime ParsedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// The slicer software used to generate the GCODE
        /// </summary>
        public string? SlicerSoftware { get; set; } = string.Empty;

        /// <summary>
        /// Base64 encoded thumbnail image from the GCODE file
        /// </summary>
        public string? ThumbnailBase64 { get; set; } = string.Empty;
    }
}

