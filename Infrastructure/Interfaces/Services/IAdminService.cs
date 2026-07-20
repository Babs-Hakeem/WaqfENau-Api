using WaqfENau.Api.DTOs;

namespace WaqfENau.Api.Infrastructure.Interfaces.Services
{
    public interface IAdminService
    {
        // Section
        Task<SectionResponse> CreateSectionAsync(CreateSectionRequest request);
        Task<SectionResponse> UpdateSectionAsync(Guid sectionId, UpdateSectionRequest request);
        Task<bool> PublishSectionAsync(Guid sectionId);
        Task<bool> UnpublishSectionAsync(Guid sectionId);
        Task<bool> DeleteSectionAsync(Guid sectionId);
        Task<List<SectionResponse>> GetAllSectionsAsync();
        Task<SectionResponse?> GetSectionByIdAsync(Guid sectionId);

        // Unit
        Task<UnitResponse> CreateUnitAsync(CreateUnitRequest request);
        Task<UnitResponse> UpdateUnitAsync(Guid unitId, UpdateUnitRequest request);
        Task<bool> PublishUnitAsync(Guid unitId);
        Task<bool> UnpublishUnitAsync(Guid unitId);
        Task<bool> DeleteUnitAsync(Guid unitId);
        Task<List<UnitResponse>> GetUnitsBySectionAsync(Guid sectionId);
        Task<UnitResponse?> GetUnitByIdAsync(Guid unitId);

        // Lesson
        Task<AdminLessonResponse> CreateLessonAsync(CreateLessonRequest request);
        Task<AdminLessonResponse> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request);
        Task<bool> DeleteLessonAsync(Guid lessonId);
        Task<List<AdminLessonResponse>> GetLessonsByUnitAsync(Guid unitId);
        Task<AdminLessonResponse?> GetLessonByIdAsync(Guid lessonId);

        // Exercise
        Task<ExerciseResponse> CreateExerciseAsync(CreateExerciseRequest request);
        Task<ExerciseResponse> UpdateExerciseAsync(Guid exerciseId, UpdateExerciseRequest request);
        Task<bool> DeleteExerciseAsync(Guid exerciseId);
        Task<List<ExerciseResponse>> GetExercisesByLessonAsync(Guid lessonId);

        // Branch / Achievement (unchanged)
        Task<Guid> CreateBranchAsync(CreateBranchRequest request);
        Task<Guid> CreateAchievementAsync(CreateAchievementRequest request);
    }
}
