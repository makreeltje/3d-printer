namespace _3d_printer_cost_calculator.Models;

public class ParsedGcode
{
    public double FilamentUsedMm { get; set; }
    public double FilamentUsedGrams { get; set; }
    public TimeSpan EstimatedPrintTime { get; set; }
    public int LayerCount { get; set; }
    public double LayerHeight { get; set; }
    public int NozzleTemperature { get; set; }
    public int BedTemperature { get; set; }
    public string SlicerName { get; set; } = string.Empty;
    public string SlicerVersion { get; set; } = string.Empty;
}