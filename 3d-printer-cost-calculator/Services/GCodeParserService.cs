using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Services
{
    public class GCodeParserService
    {
        private readonly List<IGCodeParser> _parsers;

        public GCodeParserService()
        {
            _parsers = new List<IGCodeParser>
            {
                new BambuStudioGCodeParser(),
                new CuraGCodeParser(),
                new OrcaSlicerGCodeParser(),
                new CrealityPrintGCodeParser(),

            };
        }

        public async Task<GCodeParseResult> ParseAsync(Stream gcodeStream)
        {
            using var reader = new StreamReader(gcodeStream);
            var lines = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null)
                    lines.Add(line);
            }
            var parser = _parsers.Find(p => p.CanParse(lines));
            if (parser == null)
                throw new NotSupportedException("The delivered G-code is not supported by this service.");
            return parser.Parse(lines);
        }
    }
}
