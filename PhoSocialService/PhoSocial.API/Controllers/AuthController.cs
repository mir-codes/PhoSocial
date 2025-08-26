using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PhoSocial.API.DTOs;
using PhoSocial.API.Services;

namespace PhoSocial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) { _auth = auth; }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupDto dto)
        {
            try
            {
                var token = await _auth.SignupAsync(dto);
                return Ok(new { token });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _auth.LoginAsync(dto);
                return Ok(new { token });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
