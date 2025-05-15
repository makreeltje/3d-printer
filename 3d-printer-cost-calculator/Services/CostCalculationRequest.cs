namespace Services;

public class CostCalculationRequest
{
    public double FilamentUsedGrams { get; set; }
    public double PrintTimeHours { get; set; }
    public double PricePerKg { get; set; }
    public double PricePerKWh { get; set; }
    public double PrinterCost { get; set; }
    public double PrinterLifespanHours { get; set; }
    public double PrinterPowerKw { get; set; }
    public double FailureRate { get; set; } // e.g., 0.1 for 10%
}