using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WaqfENau.Api.Infrastructure.Interfaces.Services;

namespace WaqfENau.Api.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard([FromQuery] string scope = "Branch")
        {
            var memberId = GetMemberIdFromToken();
            if (memberId == null) return Unauthorized();

            if (scope != "Branch" && scope != "National")
                return BadRequest(new { message = "Scope must be 'Branch' or 'National'" });

            var leaderboard = await _leaderboardService.GetLeaderboardAsync(memberId.Value, scope);
            return Ok(leaderboard);
        }

        private Guid? GetMemberIdFromToken()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? Guid.Parse(claim) : null;
        }
    }
}
