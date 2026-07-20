using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WaqfENau.Api.DTOs;
using WaqfENau.Api.Infrastructure.Interfaces.Services;

namespace WaqfENau.Api.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonsController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpGet("unit/{unitId}")]
        public async Task<IActionResult> GetLessonsByUnit(Guid unitId)
        {
            var memberId = GetMemberIdFromToken();
            if (memberId == null) return Unauthorized();

            var lessons = await _lessonService.GetLessonsByUnitAsync(memberId.Value, unitId);
            return Ok(lessons);
        }

        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLesson(Guid lessonId)
        {
            var memberId = GetMemberIdFromToken();
            if (memberId == null) return Unauthorized();

            var lesson = await _lessonService.GetLessonDetailAsync(memberId.Value, lessonId);
            if (lesson == null) return NotFound();

            return Ok(lesson);
        }

        [HttpPost("exercises/answer")]
        public async Task<IActionResult> SubmitExerciseAnswer([FromBody] SubmitExerciseAnswerRequest request)
        {
            var memberId = GetMemberIdFromToken();
            if (memberId == null) return Unauthorized();

            try
            {
                var result = await _lessonService.SubmitExerciseAnswerAsync(memberId.Value, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteLesson([FromBody] CompleteLessonRequest request)
        {
            var memberId = GetMemberIdFromToken();
            if (memberId == null) return Unauthorized();

            try
            {
                var result = await _lessonService.CompleteLessonAsync(memberId.Value, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid? GetMemberIdFromToken()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? Guid.Parse(claim) : null;
        }
    }
}
