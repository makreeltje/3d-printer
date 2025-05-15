using _3d_printer_cost_calculator.Models;

namespace Services.Parser;

public class GcodeParserSelector
{
    private readonly IEnumerable<IGcodeParser> _parsers;

    public GcodeParserSelector(IEnumerable<IGcodeParser> parsers)
    {
        _parsers = parsers;
    }

    public ParsedGcode Parse(string[] gcodelines)
    {
        foreach (var parser in _parsers)
        {
            if (parser.CanParse(gcodelines))
                return parser.Parse(gcodelines);
        }
        
        throw new NotSupportedException("Unsupported GCODE format");
    }
}