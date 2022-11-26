using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private string[] oKResponses = new string[5]
        {
            "Yes!",
            "Yes I am!",
            "I wouldn't call it 'living' but it'll do",
            "Physically, yes",
            "Yep"
        };

        [HttpGet]
        public async Task<ActionResult<string>> IsServiceAlive()
        {
            Random random = new Random();

            string response = oKResponses[random.Next(0, 4)];

            return Ok(response);
        }
    }
}
