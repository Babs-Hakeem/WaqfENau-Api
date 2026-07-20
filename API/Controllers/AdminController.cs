using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfENau.Api.DTOs;
using WaqfENau.Api.Infrastructure.Interfaces.Services;

namespace WaqfENau.Api.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "NationalAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ═══════════════════════════════════════════════════════════
        // SECTIONS  →  Step 1: Create a Section (like a "Course")
        // ═══════════════════════════════════════════════════════════

        [HttpPost("sections")]
        public async Task<IActionResult> CreateSection([FromBody] CreateSectionRequest request)
        {
            try
            {
                var result = await _adminService.CreateSectionAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("sections")]
        public async Task<IActionResult> GetAllSections()
        {
            var sections = await _adminService.GetAllSectionsAsync();
            return Ok(sections);
        }

        [HttpGet("sections/{sectionId}")]
        public async Task<IActionResult> GetSection(Guid sectionId)
        {
            var section = await _adminService.GetSectionByIdAsync(sectionId);
            if (section == null) return NotFound();
            return Ok(section);
        }

        [HttpPut("sections/{sectionId}")]
        public async Task<IActionResult> UpdateSection(Guid sectionId, [FromBody] UpdateSectionRequest request)
        {
            try
            {
                var result = await _adminService.UpdateSectionAsync(sectionId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("sections/{sectionId}/publish")]
        public async Task<IActionResult> PublishSection(Guid sectionId)
        {
            var success = await _adminService.PublishSectionAsync(sectionId);
            if (!success) return NotFound();
            return Ok(new { message = "Section published" });
        }

        [HttpPost("sections/{sectionId}/unpublish")]
        public async Task<IActionResult> UnpublishSection(Guid sectionId)
        {
            var success = await _adminService.UnpublishSectionAsync(sectionId);
            if (!success) return NotFound();
            return Ok(new { message = "Section moved back to draft" });
        }

        [HttpDelete("sections/{sectionId}")]
        public async Task<IActionResult> DeleteSection(Guid sectionId)
        {
            var success = await _adminService.DeleteSectionAsync(sectionId);
            if (!success) return NotFound();
            return Ok(new { message = "Section deleted" });
        }

        // ═══════════════════════════════════════════════════════════
        // UNITS  →  Step 2: Add Units under a Section (like "Modules")
        // ═══════════════════════════════════════════════════════════

        [HttpPost("units")]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitRequest request)
        {
            try
            {
                var result = await _adminService.CreateUnitAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("sections/{sectionId}/units")]
        public async Task<IActionResult> GetUnitsBySection(Guid sectionId)
        {
            var units = await _adminService.GetUnitsBySectionAsync(sectionId);
            return Ok(units);
        }

        [HttpGet("units/{unitId}")]
        public async Task<IActionResult> GetUnit(Guid unitId)
        {
            var unit = await _adminService.GetUnitByIdAsync(unitId);
            if (unit == null) return NotFound();
            return Ok(unit);
        }

        [HttpPut("units/{unitId}")]
        public async Task<IActionResult> UpdateUnit(Guid unitId, [FromBody] UpdateUnitRequest request)
        {
            try
            {
                var result = await _adminService.UpdateUnitAsync(unitId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("units/{unitId}/publish")]
        public async Task<IActionResult> PublishUnit(Guid unitId)
        {
            var success = await _adminService.PublishUnitAsync(unitId);
            if (!success) return NotFound();
            return Ok(new { message = "Unit published" });
        }

        [HttpPost("units/{unitId}/unpublish")]
        public async Task<IActionResult> UnpublishUnit(Guid unitId)
        {
            var success = await _adminService.UnpublishUnitAsync(unitId);
            if (!success) return NotFound();
            return Ok(new { message = "Unit moved back to draft" });
        }

        [HttpDelete("units/{unitId}")]
        public async Task<IActionResult> DeleteUnit(Guid unitId)
        {
            var success = await _adminService.DeleteUnitAsync(unitId);
            if (!success) return NotFound();
            return Ok(new { message = "Unit deleted" });
        }

        // ═══════════════════════════════════════════════════════════
        // LESSONS  →  Step 3: Add Lessons under a Unit
        // ═══════════════════════════════════════════════════════════

        [HttpPost("lessons")]
        public async Task<IActionResult> CreateLesson([FromBody] CreateLessonRequest request)
        {
            try
            {
                var result = await _adminService.CreateLessonAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("units/{unitId}/lessons")]
        public async Task<IActionResult> GetLessonsByUnit(Guid unitId)
        {
            var lessons = await _adminService.GetLessonsByUnitAsync(unitId);
            return Ok(lessons);
        }

        [HttpGet("lessons/{lessonId}")]
        public async Task<IActionResult> GetLesson(Guid lessonId)
        {
            var lesson = await _adminService.GetLessonByIdAsync(lessonId);
            if (lesson == null) return NotFound();
            return Ok(lesson);
        }

        [HttpPut("lessons/{lessonId}")]
        public async Task<IActionResult> UpdateLesson(Guid lessonId, [FromBody] UpdateLessonRequest request)
        {
            try
            {
                var result = await _adminService.UpdateLessonAsync(lessonId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("lessons/{lessonId}")]
        public async Task<IActionResult> DeleteLesson(Guid lessonId)
        {
            var success = await _adminService.DeleteLessonAsync(lessonId);
            if (!success) return NotFound();
            return Ok(new { message = "Lesson deleted" });
        }

        // ═══════════════════════════════════════════════════════════
        // EXERCISES  →  Step 4: Build Exercises under a Lesson
        // ═══════════════════════════════════════════════════════════

        [HttpPost("exercises")]
        public async Task<IActionResult> CreateExercise([FromBody] CreateExerciseRequest request)
        {
            try
            {
                var result = await _adminService.CreateExerciseAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("lessons/{lessonId}/exercises")]
        public async Task<IActionResult> GetExercisesByLesson(Guid lessonId)
        {
            var exercises = await _adminService.GetExercisesByLessonAsync(lessonId);
            return Ok(exercises);
        }

        [HttpPut("exercises/{exerciseId}")]
        public async Task<IActionResult> UpdateExercise(Guid exerciseId, [FromBody] UpdateExerciseRequest request)
        {
            try
            {
                var result = await _adminService.UpdateExerciseAsync(exerciseId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("exercises/{exerciseId}")]
        public async Task<IActionResult> DeleteExercise(Guid exerciseId)
        {
            var success = await _adminService.DeleteExerciseAsync(exerciseId);
            if (!success) return NotFound();
            return Ok(new { message = "Exercise deleted" });
        }

        // ═══════════════════════════════════════════════════════════
        // BRANCH / ACHIEVEMENT (unchanged)
        // ═══════════════════════════════════════════════════════════

        [HttpPost("branches")]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request)
        {
            try
            {
                var branchId = await _adminService.CreateBranchAsync(request);
                return Ok(new { id = branchId, message = "Branch created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("achievements")]
        public async Task<IActionResult> CreateAchievement([FromBody] CreateAchievementRequest request)
        {
            try
            {
                var achievementId = await _adminService.CreateAchievementAsync(request);
                return Ok(new { id = achievementId, message = "Achievement created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
