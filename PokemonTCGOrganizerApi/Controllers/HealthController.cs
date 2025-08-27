using Microsoft.AspNetCore.Mvc;

namespace PokemonTCGOrganizer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { status = "pong" });
        }
    }
}
