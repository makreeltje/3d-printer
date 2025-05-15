namespace Services;

public class CostCalculationResult
{
    public double MaterialCost { get; set; }
    public double ElectricityCost { get; set; }
    public double DepreciationCost { get; set; }
    public double FailureRateAdjustment { get; set; }
    public double TotalCost { get; set; }
}