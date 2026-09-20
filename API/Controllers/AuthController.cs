using Microsoft.AspNetCore.Mvc;
using WaqfENau.Api.DTOs;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Infrastructure.Interfaces.Services;
using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IAuthService authService, IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Public branch list for the registration form's branch picker.
        /// Deliberately unauthenticated — a new user has no token yet.
        /// Distinct from /admin/branches, which is NationalAdmin-only.
        /// </summary>
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _unitOfWork.Repository<Branch>().GetAllAsync();
            var result = branches
                .OrderBy(b => b.Name)
                .Select(b => new { id = b.Id, name = b.Name, city = b.City, state = b.State });
            return Ok(result);
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var memberIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(memberIdClaim))
                return Unauthorized();

            await _authService.LogoutAsync(Guid.Parse(memberIdClaim));
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
