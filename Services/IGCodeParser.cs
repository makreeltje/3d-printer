namespace Services;

public interface IGCodeParser
{
    bool CanParse(IEnumerable<string> lines);
    GCodeParseResult Parse(IEnumerable<string> lines);
}
