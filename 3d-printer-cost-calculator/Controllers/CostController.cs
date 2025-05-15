using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CostController : ControllerBase
    {
        [HttpPost("calculate")]
        public ActionResult<CostCalculationResult> Calculate([FromBody] CostCalculationRequest request)
        {
            var result = CostCalculator.Calculate(request);
            return Ok(result);
        }
    }
}