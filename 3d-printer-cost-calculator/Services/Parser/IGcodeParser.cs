using _3d_printer_cost_calculator.Models;

namespace Services.Parser;

public interface IGcodeParser
{
    bool CanParse(string[] gcodeLines);
    ParsedGcode Parse(string[] gcodeLines);
}