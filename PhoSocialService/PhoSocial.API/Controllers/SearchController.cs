using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PhoSocial.API.Repositories;

namespace PhoSocial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class SearchController : ControllerBase
    {
        private readonly IUserRepository _users;
        public SearchController(IUserRepository users) { _users = users; }

        [HttpGet("users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q)
        {
            var results = await _users.SearchByNameOrEmailAsync(q ?? string.Empty);
            return Ok(results);
        }
    }
}
