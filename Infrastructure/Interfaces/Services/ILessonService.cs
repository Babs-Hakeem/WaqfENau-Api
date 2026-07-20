using WaqfENau.Api.DTOs;
namespace WaqfENau.Api.Infrastructure.Interfaces.Services
{
    public interface ILessonService
    {
        Task<List<LessonResponse>> GetLessonsByUnitAsync(Guid memberId, Guid unitId);
        Task<LessonDetailResponse?> GetLessonDetailAsync(Guid memberId, Guid lessonId);
        Task<SubmitExerciseAnswerResponse> SubmitExerciseAnswerAsync(Guid memberId, SubmitExerciseAnswerRequest request);
        Task<CompleteLessonResponse> CompleteLessonAsync(Guid memberId, CompleteLessonRequest request);
    }
}
