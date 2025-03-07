using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThreeDPrintCostCalculator.Models;
using ThreeDPrintCostCalculator.Services;

namespace ThreeDPrintCostCalculator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFileParsingService _fileParsingService;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileParsingService fileParsingService, ILogger<FileController> logger)
        {
            _fileParsingService = fileParsingService ?? throw new ArgumentNullException(nameof(fileParsingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Uploads and processes a 3MF file, returning the parsed model data
        /// </summary>
        /// <returns>The parsed 3MF model data</returns>
        /// <response code="200">Returns the parsed model data</response>
        /// <response code="400">If the file is missing, empty, or invalid</response>
        /// <response code="415">If the file is not a 3MF file</response>
        /// <response code="500">If there was an error processing the file</response>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(ThreeMFModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file was uploaded or file is empty");
                    return BadRequest("Please upload a file");
                }

                // Validate file extension
                string fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (fileExtension != ".3mf")
                {
                    _logger.LogWarning("Attempted upload of unsupported file type: {FileExtension}", fileExtension);
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType, "Only 3MF files are supported");
                }

                // Process the file
                using (var stream = file.OpenReadStream())
                {
                    var model = await _fileParsingService.Parse3MFFileAsync(stream);
                    
                    if (model == null)
                    {
                        _logger.LogWarning("Failed to parse 3MF file: {FileName}", file.FileName);
                        return BadRequest("Could not parse the 3MF file. The file may be corrupt or invalid.");
                    }

                    _logger.LogInformation("Successfully parsed 3MF file: {FileName}", file.FileName);
                    return Ok(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing 3MF file upload");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the file");
            }
        }

        /// <summary>
        /// Validates a 3MF file without fully processing it
        /// </summary>
        /// <returns>Validation result</returns>
        /// <response code="200">If the file is a valid 3MF file</response>
        /// <response code="400">If the file is missing, empty, or invalid</response>
        /// <response code="415">If the file is not a 3MF file</response>
        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        public async Task<IActionResult> ValidateFile(IFormFile file)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Please upload a file");
                }

                // Validate file extension
                string fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (fileExtension != ".3mf")
                {
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType, "Only 3MF files are supported");
                }

                // Validate the file content
                using (var stream = file.OpenReadStream())
                {
                    bool isValid = await _fileParsingService.Validate3MFFileAsync(stream);
                    if (!isValid)
                    {
                        return BadRequest("The file is not a valid 3MF file");
                    }

                    return Ok(new { isValid = true, message = "File is a valid 3MF file" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating 3MF file");
                return BadRequest("Could not validate the file");
            }
        }
    }
}

