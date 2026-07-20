using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WaqfENau.Api.Infrastructure.Interfaces.Services;

namespace WaqfENau.Api.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Murabbi")]
    public class MurabbiController : ControllerBase
    {
        private readonly IMurabbiService _murabbiService;

        public MurabbiController(IMurabbiService murabbiService)
        {
            _murabbiService = murabbiService;
        }

        [HttpGet("members")]
        public async Task<IActionResult> GetBranchMembers()
        {
            var murabbiId = GetMemberIdFromToken();
            if (murabbiId == null) return Unauthorized();

            try
            {
                var members = await _murabbiService.GetBranchMembersAsync(murabbiId.Value);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return Forbid();
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetBranchSummary()
        {
            var murabbiId = GetMemberIdFromToken();
            if (murabbiId == null) return Unauthorized();

            try
            {
                var summary = await _murabbiService.GetBranchSummaryAsync(murabbiId.Value);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return Forbid();
            }
        }

        [HttpGet("members/{memberId}")]
        public async Task<IActionResult> GetMemberDetails(Guid memberId)
        {
            var murabbiId = GetMemberIdFromToken();
            if (murabbiId == null) return Unauthorized();

            try
            {
                var details = await _murabbiService.GetMemberDetailsAsync(murabbiId.Value, memberId);
                if (details == null) return NotFound();
                return Ok(details);
            }
            catch (Exception ex)
            {
                return Forbid();
            }
        }

        private Guid? GetMemberIdFromToken()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? Guid.Parse(claim) : null;
        }
    }

}
