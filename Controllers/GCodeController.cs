using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.IO;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GCodeController : ControllerBase
    {
        private readonly GCodeParserService _parserService;

        public GCodeController()
        {
            _parserService = new GCodeParserService();
        }

        [HttpPost("parse")]
        public async Task<IActionResult> ParseGCode([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _parserService.ParseAsync(stream);
                return Ok(result);
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}
