using System;

namespace _3d_printer_cost_calculator.Models
{
    public class CostCalculation
    {
        /// <summary>
        /// Cost of the filament material used
        /// </summary>
        public decimal MaterialCost { get; set; }

        /// <summary>
        /// Cost of electricity consumed during printing
        /// </summary>
        public decimal ElectricityCost { get; set; }

        /// <summary>
        /// Cost attributed to printer depreciation
        /// </summary>
        public decimal DepreciationCost { get; set; }

        /// <summary>
        /// Cost of labor for setup, monitoring, and post-processing
        /// </summary>
        public decimal LaborCost { get; set; }

        /// <summary>
        /// Additional costs (e.g., failed print risk, maintenance)
        /// </summary>
        public decimal AdditionalCosts { get; set; }

        /// <summary>
        /// Sum of all costs
        /// </summary>
        public decimal TotalCost => MaterialCost + ElectricityCost + DepreciationCost + LaborCost + AdditionalCosts;

        /// <summary>
        /// Currency code (e.g., USD, EUR)
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// The GCODE file associated with this cost calculation
        /// </summary>
        public GCodeFile GCodeFile { get; set; }

        /// <summary>
        /// The date when the calculation was performed
        /// </summary>
        public DateTime CalculationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Optional notes about the calculation
        /// </summary>
        public string Notes { get; set; }
    }
}

