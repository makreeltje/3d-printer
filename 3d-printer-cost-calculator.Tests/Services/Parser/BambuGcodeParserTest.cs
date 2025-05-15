using System.IO;
using FluentAssertions;
using Services.Parser;
using Xunit;

namespace _3d_printer_cost_calculator.Tests.Services.Parser;

public class BambuGcodeParserTests
{
    [Fact]
    public void Parses_Valid_BambuStudio_Gcode_Correctly()
    {
        var lines = File.ReadAllLines("../../../Examples/bambu_lab.gcode");
        var parser = new BambuGcodeParser();

        var result = parser.Parse(lines);

        result.FilamentUsedMm.Should().BeApproximately(1503.61, 0.01);
        result.FilamentUsedGrams.Should().BeApproximately(4.48, 0.01);
        result.LayerCount.Should().Be(25);
        result.NozzleTemperature.Should().Be(220);
        result.BedTemperature.Should().Be(60);
        result.SlicerName.Should().Be("Bambu Studio");
    }
    
    [Fact]
    public void Parses_Gcode_With_Missing_Optional_Fields()
    {
        var gcode = new[]
        {
            "; total filament length [mm] : 123.45",
            "; estimated printing time (normal mode) = 4h 23m",
            "; total layer number: 20",
            "START_PRINT EXTRUDER_TEMP=210 BED_TEMP=55"
        };

        var parser = new BambuGcodeParser();
        var result = parser.Parse(gcode);

        result.FilamentUsedMm.Should().BeApproximately(123.45, 0.01);
        result.FilamentUsedGrams.Should().BeNull(); // Optional
        result.EstimatedPrintTime.TotalMinutes.Should().BeGreaterThan(260);
        result.LayerCount.Should().Be(20);
        result.NozzleTemperature.Should().Be(210);
        result.BedTemperature.Should().Be(55);
    }
}