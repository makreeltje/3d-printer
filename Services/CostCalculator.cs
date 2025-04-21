namespace Services;

public static class CostCalculator
{
    public static CostCalculationResult Calculate(CostCalculationRequest req)
    {
        double materialCost = req.FilamentUsedGrams / 1000.0 * req.PricePerKg;
        double electricityCost = req.PrintTimeHours * req.PrinterPowerKw * req.PricePerKWh;
        double depreciationCost = (req.PrinterCost / req.PrinterLifespanHours) * req.PrintTimeHours;
        double subtotal = materialCost + electricityCost + depreciationCost;
        double totalCost = subtotal / (1.0 - req.FailureRate);
        double failureAdjustment = totalCost - subtotal;

        return new CostCalculationResult
        {
            MaterialCost = Math.Round(materialCost, 2),
            ElectricityCost = Math.Round(electricityCost, 2),
            DepreciationCost = Math.Round(depreciationCost, 2),
            FailureRateAdjustment = Math.Round(failureAdjustment, 2),
            TotalCost = Math.Round(totalCost, 2)
        };
    }
}