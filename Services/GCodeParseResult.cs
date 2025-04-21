using System;
using System.Collections.Generic;

namespace Services;

public class GCodeParseResult
{
    public string? Slicer { get; set; }
    public double FilamentUsedGrams { get; set; }
    public double FilamentUsedMm { get; set; }
    public TimeSpan? EstimatedPrintTime { get; set; }
    public int? LayerCount { get; set; }
}
