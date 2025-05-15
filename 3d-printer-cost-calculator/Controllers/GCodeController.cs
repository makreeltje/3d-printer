using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.IO;
using System.Threading.Tasks;
using _3d_printer_cost_calculator.Models;
using Services.Parser;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GCodeController : ControllerBase
    {
        private readonly GcodeParserSelector _parserSelector;

        public GCodeController(GcodeParserSelector parserSelector)
        {
            _parserSelector = parserSelector;
        }

        [HttpPost("parse")]
        public async Task<ActionResult<ParsedGcode>> ParseGcode(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }
            
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            var lines = content.Split('\n', '\r').Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();

            try
            {
                var parsed = _parserSelector.Parse(lines);
                return Ok(parsed);
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
