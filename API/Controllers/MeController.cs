using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WaqfENau.Api.DTOs;
using WaqfENau.Api.Infrastructure.Interfaces.Services;

namespace WaqfENau.Api.API.Controllers
{
    [ApiController]
    [Route("api/me")]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly IMeService _meService;
        private readonly IFriendsService _friendsService;

        public MeController(IMeService meService, IFriendsService friendsService)
        {
            _meService = meService;
            _friendsService = friendsService;
        }

        // ── Profile ──────────────────────────────────────────────────

        /// <summary>
        /// Returns the full profile of the currently logged-in member
        /// including streak, hearts, XP, level, branch, and progress summary.
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            try
            {
                var profile = await _meService.GetProfileAsync(memberId.Value);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Set the member's daily study goal (5/10/15/20 minutes).</summary>
        [HttpPatch("daily-goal")]
        public async Task<IActionResult> UpdateDailyGoal([FromBody] UpdateDailyGoalRequest request)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            try
            {
                await _meService.UpdateDailyGoalAsync(memberId.Value, request.GoalMinutes);
                return Ok(new { message = "Daily goal updated" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Hearts ───────────────────────────────────────────────────

        /// <summary>Returns current hearts count and next refill time.</summary>
        [HttpGet("hearts")]
        public async Task<IActionResult> GetHearts()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var hearts = await _meService.GetHeartsAsync(memberId.Value);
            return Ok(hearts);
        }

        // ── Streak ───────────────────────────────────────────────────

        /// <summary>Returns current streak, longest streak, freezes available.</summary>
        [HttpGet("streak")]
        public async Task<IActionResult> GetStreak()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var streak = await _meService.GetStreakAsync(memberId.Value);
            return Ok(streak);
        }

        // ── Learning Path ─────────────────────────────────────────────

        /// <summary>
        /// Returns the full Section → Unit path map for this member's age group.
        /// Each unit shows progress%, isUnlocked, isCompleted.
        /// This is the main home screen data.
        /// </summary>
        [HttpGet("path")]
        public async Task<IActionResult> GetLearningPath()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            try
            {
                var path = await _meService.GetMyLearningPathAsync(memberId.Value);
                return Ok(path);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Friends ───────────────────────────────────────────────────

        /// <summary>Send a friend request to another member.</summary>
        [HttpPost("friends/request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestRequest request)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            try
            {
                await _friendsService.SendFriendRequestAsync(memberId.Value, request.ReceiverId);
                return Ok(new { message = "Friend request sent" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Accept a pending friend request.</summary>
        [HttpPost("friends/{friendshipId}/accept")]
        public async Task<IActionResult> AcceptFriendRequest(Guid friendshipId)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            try
            {
                await _friendsService.AcceptFriendRequestAsync(memberId.Value, friendshipId);
                return Ok(new { message = "Friend request accepted" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Decline or remove a friend.</summary>
        [HttpDelete("friends/{friendshipId}")]
        public async Task<IActionResult> RemoveFriend(Guid friendshipId)
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            try
            {
                await _friendsService.RemoveFriendAsync(memberId.Value, friendshipId);
                return Ok(new { message = "Friend removed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Get all accepted friends with their streak and XP.</summary>
        [HttpGet("friends")]
        public async Task<IActionResult> GetFriends()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var friends = await _friendsService.GetFriendsAsync(memberId.Value);
            return Ok(friends);
        }

        /// <summary>Get all pending incoming friend requests.</summary>
        [HttpGet("friends/requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var requests = await _friendsService.GetPendingRequestsAsync(memberId.Value);
            return Ok(requests);
        }

        /// <summary>Weekly friend leaderboard — resets every Monday.</summary>
        [HttpGet("friends/leaderboard")]
        public async Task<IActionResult> GetFriendLeaderboard()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var leaderboard = await _friendsService.GetFriendLeaderboardAsync(memberId.Value);
            return Ok(leaderboard);
        }

        // ── Helper ───────────────────────────────────────────────────

        private Guid? GetMemberId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? Guid.Parse(claim) : null;
        }
    }
}
