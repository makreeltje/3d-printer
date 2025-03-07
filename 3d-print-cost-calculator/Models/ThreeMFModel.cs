using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThreeDPrintCostCalculator.Models
{
    /// <summary>
    /// Represents metadata and properties of a 3MF (3D Manufacturing Format) file
    /// </summary>
    public class ThreeMFModel
    {
        /// <summary>
        /// Unique identifier for the model
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Name of the 3D model
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = "Unnamed 3D Model";

        /// <summary>
        /// Original filename of the 3MF file
        /// </summary>
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = "untitled.3mf";

        /// <summary>
        /// Size of the 3MF file in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Date and time when the model was uploaded or imported
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Width of the model in the specified unit
        /// </summary>
        [Range(0.001, double.MaxValue, ErrorMessage = "Width must be greater than 0")]
        public double Width { get; set; }

        /// <summary>
        /// Height of the model in the specified unit
        /// </summary>
        [Range(0.001, double.MaxValue, ErrorMessage = "Height must be greater than 0")]
        public double Height { get; set; }

        /// <summary>
        /// Depth of the model in the specified unit
        /// </summary>
        [Range(0.001, double.MaxValue, ErrorMessage = "Depth must be greater than 0")]
        public double Depth { get; set; }

        /// <summary>
        /// Volume of the model in cubic units
        /// </summary>
        [Range(0.001, double.MaxValue, ErrorMessage = "Volume must be greater than 0")]
        public double Volume { get; set; }

        /// <summary>
        /// Surface area of the model in square units
        /// </summary>
        [Range(0.001, double.MaxValue, ErrorMessage = "Surface area must be greater than 0")]
        public double SurfaceArea { get; set; }

        /// <summary>
        /// Units used for measurements (e.g., mm, cm, inches)
        /// </summary>
        [Required]
        public string Units { get; set; } = "mm";

        /// <summary>
        /// Number of triangles in the mesh
        /// </summary>
        public int TriangleCount { get; set; }

        /// <summary>
        /// Number of materials used in the model
        /// </summary>
        public int MaterialCount { get; set; }

        /// <summary>
        /// List of material names used in the model, separated by commas
        /// </summary>
        [StringLength(1000)]
        public string Materials { get; set; } = "Unknown Material";

        /// <summary>
        /// Resolution or layer height recommended for printing
        /// </summary>
        public double? RecommendedLayerHeight { get; set; }

        /// <summary>
        /// Indicates if the model has been verified as printable
        /// </summary>
        public bool IsPrintable { get; set; }

        /// <summary>
        /// Indicates if the model is manifold (watertight)
        /// </summary>
        public bool IsManifold { get; set; }

        /// <summary>
        /// Any errors or warnings detected in the model
        /// </summary>
        [StringLength(2000)]
        public string ModelWarnings { get; set; } = "No warnings detected";

        /// <summary>
        /// Path to the thumbnail image of the model, if available
        /// </summary>
        [StringLength(1000)]
        public string ThumbnailPath { get; set; } = "default-thumbnail.png";

        /// <summary>
        /// Estimated printing time in minutes
        /// </summary>
        public double? EstimatedPrintTime { get; set; }

        /// <summary>
        /// Estimated material usage in grams
        /// </summary>
        public double? EstimatedMaterialUsage { get; set; }

        /// <summary>
        /// Estimated cost based on material usage and print time
        /// </summary>
        [JsonIgnore]
        public decimal? EstimatedCost => CalculateEstimatedCost();

        /// <summary>
        /// Calculate the estimated cost based on material usage and print time
        /// This is a placeholder method that should be implemented with actual cost calculation logic
        /// </summary>
        /// <returns>The estimated cost of printing the model</returns>
        private decimal? CalculateEstimatedCost()
        {
            // This is a placeholder for the actual cost calculation logic
            // The real implementation would depend on your specific pricing model
            if (EstimatedMaterialUsage.HasValue && EstimatedPrintTime.HasValue)
            {
                // Example calculation: $0.05 per gram of material + $0.10 per minute of print time
                return (decimal)(EstimatedMaterialUsage.Value * 0.05 + EstimatedPrintTime.Value * 0.10);
            }
            return null;
        }
    }
}

