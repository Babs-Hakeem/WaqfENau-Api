using WaqfENau.Api.DTOs;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Infrastructure.Interfaces.Services;
using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Implementation.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ═══════════════════════════════════════════════════════════
        // SECTION
        // ═══════════════════════════════════════════════════════════

        public async Task<SectionResponse> CreateSectionAsync(CreateSectionRequest request)
        {
            var section = new Section
            {
                Title = request.Title,
                Description = request.Description,
                AgeGroup = request.AgeGroup,
                OrderIndex = request.OrderIndex,
                IsPublished = false,
                IsActive = true
            };

            await _unitOfWork.Sections.AddAsync(section);
            await _unitOfWork.SaveChangesAsync();

            return MapSection(section, 0);
        }

        public async Task<SectionResponse> UpdateSectionAsync(Guid sectionId, UpdateSectionRequest request)
        {
            var section = await _unitOfWork.Sections.GetByIdAsync(sectionId)
                ?? throw new Exception("Section not found");

            section.Title = request.Title;
            section.Description = request.Description;
            section.OrderIndex = request.OrderIndex;
            section.IsActive = request.IsActive;
            section.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Sections.Update(section);
            await _unitOfWork.SaveChangesAsync();

            var withUnits = await _unitOfWork.Sections.GetByIdWithUnitsAsync(sectionId);
            return MapSection(section, withUnits?.Units.Count ?? 0);
        }

        public async Task<bool> PublishSectionAsync(Guid sectionId)
        {
            var section = await _unitOfWork.Sections.GetByIdAsync(sectionId);
            if (section == null) return false;

            section.IsPublished = true;
            section.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Sections.Update(section);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnpublishSectionAsync(Guid sectionId)
        {
            var section = await _unitOfWork.Sections.GetByIdAsync(sectionId);
            if (section == null) return false;

            section.IsPublished = false;
            section.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Sections.Update(section);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSectionAsync(Guid sectionId)
        {
            var section = await _unitOfWork.Sections.GetByIdAsync(sectionId);
            if (section == null) return false;

            _unitOfWork.Sections.Delete(section);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<SectionResponse>> GetAllSectionsAsync()
        {
            var sections = await _unitOfWork.Sections.GetAllWithUnitCountAsync();
            return sections.Select(s => MapSection(s, s.Units.Count)).ToList();
        }

        public async Task<SectionResponse?> GetSectionByIdAsync(Guid sectionId)
        {
            var section = await _unitOfWork.Sections.GetByIdWithUnitsAsync(sectionId);
            return section == null ? null : MapSection(section, section.Units.Count);
        }

        // ═══════════════════════════════════════════════════════════
        // UNIT
        // ═══════════════════════════════════════════════════════════

        public async Task<UnitResponse> CreateUnitAsync(CreateUnitRequest request)
        {
            var sectionExists = await _unitOfWork.Sections.GetByIdAsync(request.SectionId);
            if (sectionExists == null)
                throw new Exception("Section not found");

            var unit = new Unit
            {
                Title = request.Title,
                Description = request.Description,
                GuidebookContent = request.GuidebookContent,
                SectionId = request.SectionId,
                Category = request.Category,
                OrderIndex = request.OrderIndex,
                XpReward = request.XpReward,
                IsPublished = false,
                IsActive = true
            };

            await _unitOfWork.Units.AddAsync(unit);
            await _unitOfWork.SaveChangesAsync();

            return MapUnit(unit, 0);
        }

        public async Task<UnitResponse> UpdateUnitAsync(Guid unitId, UpdateUnitRequest request)
        {
            var unit = await _unitOfWork.Units.GetByIdAsync(unitId)
                ?? throw new Exception("Unit not found");

            unit.Title = request.Title;
            unit.Description = request.Description;
            unit.GuidebookContent = request.GuidebookContent;
            unit.Category = request.Category;
            unit.OrderIndex = request.OrderIndex;
            unit.XpReward = request.XpReward;
            unit.IsActive = request.IsActive;
            unit.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Units.Update(unit);
            await _unitOfWork.SaveChangesAsync();

            var withLessons = await _unitOfWork.Units.GetByIdWithLessonsAsync(unitId);
            return MapUnit(unit, withLessons?.Lessons.Count ?? 0);
        }

        public async Task<bool> PublishUnitAsync(Guid unitId)
        {
            var unit = await _unitOfWork.Units.GetByIdAsync(unitId);
            if (unit == null) return false;

            unit.IsPublished = true;
            unit.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Units.Update(unit);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnpublishUnitAsync(Guid unitId)
        {
            var unit = await _unitOfWork.Units.GetByIdAsync(unitId);
            if (unit == null) return false;

            unit.IsPublished = false;
            unit.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Units.Update(unit);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUnitAsync(Guid unitId)
        {
            var unit = await _unitOfWork.Units.GetByIdAsync(unitId);
            if (unit == null) return false;

            _unitOfWork.Units.Delete(unit);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<UnitResponse>> GetUnitsBySectionAsync(Guid sectionId)
        {
            var units = await _unitOfWork.Units.GetBySectionIdAsync(sectionId, publishedOnly: false);
            var result = new List<UnitResponse>();

            foreach (var unit in units)
            {
                var withLessons = await _unitOfWork.Units.GetByIdWithLessonsAsync(unit.Id);
                result.Add(MapUnit(unit, withLessons?.Lessons.Count ?? 0));
            }

            return result;
        }

        public async Task<UnitResponse?> GetUnitByIdAsync(Guid unitId)
        {
            var unit = await _unitOfWork.Units.GetByIdWithLessonsAsync(unitId);
            return unit == null ? null : MapUnit(unit, unit.Lessons.Count);
        }

        // ═══════════════════════════════════════════════════════════
        // LESSON
        // ═══════════════════════════════════════════════════════════

        public async Task<AdminLessonResponse> CreateLessonAsync(CreateLessonRequest request)
        {
            var unitExists = await _unitOfWork.Units.GetByIdAsync(request.UnitId);
            if (unitExists == null)
                throw new Exception("Unit not found");

            var lesson = new Lesson
            {
                Title = request.Title,
                Description = request.Description,
                UnitId = request.UnitId,
                OrderIndex = request.OrderIndex,
                XpReward = request.XpReward,
                EstimatedMinutes = request.EstimatedMinutes,
                IsActive = true
            };

            await _unitOfWork.Lessons.AddAsync(lesson);
            await _unitOfWork.SaveChangesAsync();

            return MapLesson(lesson, 0);
        }

        public async Task<AdminLessonResponse> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId)
                ?? throw new Exception("Lesson not found");

            lesson.Title = request.Title;
            lesson.Description = request.Description;
            lesson.OrderIndex = request.OrderIndex;
            lesson.XpReward = request.XpReward;
            lesson.EstimatedMinutes = request.EstimatedMinutes;
            lesson.IsActive = request.IsActive;
            lesson.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Lessons.Update(lesson);
            await _unitOfWork.SaveChangesAsync();

            var withExercises = await _unitOfWork.Lessons.GetByIdWithExercisesAsync(lessonId);
            return MapLesson(lesson, withExercises?.Exercises.Count ?? 0);
        }

        public async Task<bool> DeleteLessonAsync(Guid lessonId)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(lessonId);
            if (lesson == null) return false;

            _unitOfWork.Lessons.Delete(lesson);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<AdminLessonResponse>> GetLessonsByUnitAsync(Guid unitId)
        {
            var lessons = await _unitOfWork.Lessons.GetByUnitIdAsync(unitId);
            var result = new List<AdminLessonResponse>();

            foreach (var lesson in lessons)
            {
                var withExercises = await _unitOfWork.Lessons.GetByIdWithExercisesAsync(lesson.Id);
                result.Add(MapLesson(lesson, withExercises?.Exercises.Count ?? 0));
            }

            return result;
        }

        public async Task<AdminLessonResponse?> GetLessonByIdAsync(Guid lessonId)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdWithExercisesAsync(lessonId);
            return lesson == null ? null : MapLesson(lesson, lesson.Exercises.Count);
        }

        // ═══════════════════════════════════════════════════════════
        // EXERCISE
        // ═══════════════════════════════════════════════════════════

        public async Task<ExerciseResponse> CreateExerciseAsync(CreateExerciseRequest request)
        {
            var lessonExists = await _unitOfWork.Lessons.GetByIdAsync(request.LessonId);
            if (lessonExists == null)
                throw new Exception("Lesson not found");

            var exercise = new Exercise
            {
                LessonId = request.LessonId,
                Type = request.Type,
                OrderIndex = request.OrderIndex,
                Prompt = request.Prompt,
                ExplanationText = request.ExplanationText,
                AudioUrl = request.AudioUrl,
                ImageUrl = request.ImageUrl,
                SentenceTemplate = request.SentenceTemplate,
                XpReward = request.XpReward
            };

            await _unitOfWork.Exercises.AddAsync(exercise);
            await _unitOfWork.SaveChangesAsync();

            foreach (var opt in request.Options)
            {
                var option = new ExerciseOption
                {
                    ExerciseId = exercise.Id,
                    Text = opt.Text,
                    TextArabic = opt.TextArabic,
                    IsCorrect = opt.IsCorrect,
                    OrderIndex = opt.OrderIndex,
                    MatchGroupId = opt.MatchGroupId
                };
                await _unitOfWork.Repository<ExerciseOption>().AddAsync(option);
            }

            await _unitOfWork.SaveChangesAsync();

            var withOptions = await _unitOfWork.Exercises.GetByIdWithOptionsAsync(exercise.Id);
            return MapExercise(withOptions!);
        }

        public async Task<ExerciseResponse> UpdateExerciseAsync(Guid exerciseId, UpdateExerciseRequest request)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdWithOptionsAsync(exerciseId)
                ?? throw new Exception("Exercise not found");

            exercise.Type = request.Type;
            exercise.OrderIndex = request.OrderIndex;
            exercise.Prompt = request.Prompt;
            exercise.ExplanationText = request.ExplanationText;
            exercise.AudioUrl = request.AudioUrl;
            exercise.ImageUrl = request.ImageUrl;
            exercise.SentenceTemplate = request.SentenceTemplate;
            exercise.XpReward = request.XpReward;
            exercise.UpdatedAt = DateTime.UtcNow;

            // Replace all options (simplest approach for editing)
            var optionRepo = _unitOfWork.Repository<ExerciseOption>();
            foreach (var existingOption in exercise.Options.ToList())
                optionRepo.Delete(existingOption);

            _unitOfWork.Exercises.Update(exercise);
            await _unitOfWork.SaveChangesAsync();

            foreach (var opt in request.Options)
            {
                var option = new ExerciseOption
                {
                    ExerciseId = exercise.Id,
                    Text = opt.Text,
                    TextArabic = opt.TextArabic,
                    IsCorrect = opt.IsCorrect,
                    OrderIndex = opt.OrderIndex,
                    MatchGroupId = opt.MatchGroupId
                };
                await optionRepo.AddAsync(option);
            }

            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Exercises.GetByIdWithOptionsAsync(exerciseId);
            return MapExercise(updated!);
        }

        public async Task<bool> DeleteExerciseAsync(Guid exerciseId)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdAsync(exerciseId);
            if (exercise == null) return false;

            _unitOfWork.Exercises.Delete(exercise);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<ExerciseResponse>> GetExercisesByLessonAsync(Guid lessonId)
        {
            var exercises = await _unitOfWork.Exercises.GetByLessonIdAsync(lessonId);
            return exercises.Select(MapExercise).ToList();
        }

        // ═══════════════════════════════════════════════════════════
        // BRANCH / ACHIEVEMENT
        // ═══════════════════════════════════════════════════════════

        public async Task<Guid> CreateBranchAsync(CreateBranchRequest request)
        {
            var branch = new Branch
            {
                Name = request.Name,
                City = request.City,
                State = request.State
            };

            await _unitOfWork.Repository<Branch>().AddAsync(branch);
            await _unitOfWork.SaveChangesAsync();
            return branch.Id;
        }

        public async Task<Guid> CreateAchievementAsync(CreateAchievementRequest request)
        {
            var achievement = new Achievement
            {
                Name = request.Name,
                Description = request.Description,
                IconUrl = request.IconUrl,
                XpReward = request.XpReward,
                ConditionType = request.ConditionType,
                ConditionValue = request.ConditionValue
            };

            await _unitOfWork.Repository<Achievement>().AddAsync(achievement);
            await _unitOfWork.SaveChangesAsync();
            return achievement.Id;
        }

        // ═══════════════════════════════════════════════════════════
        // MAPPERS
        // ═══════════════════════════════════════════════════════════

        private static SectionResponse MapSection(Section s, int unitCount) => new()
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            AgeGroup = s.AgeGroup.ToString(),
            OrderIndex = s.OrderIndex,
            IsActive = s.IsActive,
            UnitCount = unitCount
        };

        private static UnitResponse MapUnit(Unit u, int lessonCount) => new()
        {
            Id = u.Id,
            Title = u.Title,
            Description = u.Description,
            GuidebookContent = u.GuidebookContent,
            SectionId = u.SectionId,
            Category = u.Category.ToString(),
            OrderIndex = u.OrderIndex,
            XpReward = u.XpReward,
            IsActive = u.IsActive,
            LessonCount = lessonCount
        };

        private static AdminLessonResponse MapLesson(Lesson l, int exerciseCount) => new()
        {
            Id = l.Id,
            Title = l.Title,
            Description = l.Description,
            UnitId = l.UnitId,
            OrderIndex = l.OrderIndex,
            XpReward = l.XpReward,
            EstimatedMinutes = l.EstimatedMinutes,
            IsActive = l.IsActive,
            ExerciseCount = exerciseCount
        };

        private static ExerciseResponse MapExercise(Exercise e) => new()
        {
            Id = e.Id,
            LessonId = e.LessonId,
            Type = e.Type.ToString(),
            OrderIndex = e.OrderIndex,
            Prompt = e.Prompt,
            ExplanationText = e.ExplanationText,
            AudioUrl = e.AudioUrl,
            ImageUrl = e.ImageUrl,
            SentenceTemplate = e.SentenceTemplate,
            XpReward = e.XpReward,
            Options = e.Options.Select(o => new ExerciseOptionResponse
            {
                Id = o.Id,
                Text = o.Text,
                TextArabic = o.TextArabic,
                IsCorrect = o.IsCorrect,
                OrderIndex = o.OrderIndex,
                MatchGroupId = o.MatchGroupId
            }).ToList()
        };
    }
}
