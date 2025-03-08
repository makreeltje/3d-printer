using System;
using System.IO;
using System.Threading.Tasks;
using _3d_printer_cost_calculator.Models;
using _3d_printer_cost_calculator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace _3d_printer_cost_calculator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GCodeController : ControllerBase
    {
        private readonly IGCodeParserService _gCodeParserService;
        private readonly ILogger<GCodeController> _logger;

        public GCodeController(IGCodeParserService gCodeParserService, ILogger<GCodeController> logger)
        {
            _gCodeParserService = gCodeParserService ?? throw new ArgumentNullException(nameof(gCodeParserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadGCodeFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                // Check if it's a gcode file
                string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension != ".gcode" && extension != ".g")
                {
                    return BadRequest("Uploaded file is not a GCODE file");
                }

                // Parse the file
                using var stream = file.OpenReadStream();
                var gCodeFile = await _gCodeParserService.ParseGCodeFileAsync(stream, file.FileName);

                return Ok(gCodeFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded GCODE file");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing the GCODE file");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetGCodeFile(Guid id)
        {
            try
            {
                // In a real application, you would retrieve the GCodeFile from a database
                // For this example, we'll just return a 404 not found
                return NotFound($"GCODE file with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving GCODE file with ID {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving the GCODE file");
            }
        }

        [HttpPost("calculate-cost")]
        public IActionResult CalculateCost([FromBody] CostCalculationRequest request)
        {
            try
            {
                if (request == null || request.GCodeFile == null)
                {
                    return BadRequest("Invalid request. GCODE file information is required.");
                }

                var costCalculation = _gCodeParserService.CalculateCost(
                    request.GCodeFile,
                    request.FilamentPricePerKg,
                    request.ElectricityPricePerKwh,
                    request.PrinterCost,
                    request.HourlyLaborRate,
                    request.PrinterPowerConsumption
                );

                return Ok(costCalculation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cost for GCODE file");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error calculating cost");
            }
        }
    }

    public class CostCalculationRequest
    {
        public GCodeFile GCodeFile { get; set; }
        public decimal FilamentPricePerKg { get; set; } = 20.0m; // Default value: $20 per kg
        public decimal ElectricityPricePerKwh { get; set; } = 0.12m; // Default value: $0.12 per kWh
        public decimal PrinterCost { get; set; } = 300.0m; // Default value: $300 printer cost
        public decimal HourlyLaborRate { get; set; } = 15.0m; // Default value: $15 per hour
        public decimal PrinterPowerConsumption { get; set; } = 150.0m; // Default value: 150W
        public string Currency { get; set; } = "USD";
    }
}

