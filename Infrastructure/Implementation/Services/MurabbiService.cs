using WaqfENau.Api.DTOs;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Infrastructure.Interfaces.Services;
using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Infrastructure.Implementation.Services
{
    public class MurabbiService : IMurabbiService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MurabbiService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MemberProgressDto>> GetBranchMembersAsync(Guid murabbiId)
        {
            var murabbi = await _unitOfWork.Members.GetByIdAsync(murabbiId);
            if (murabbi == null || murabbi.Role != UserRole.Murabbi)
                throw new Exception("Unauthorized. Only Murabbis can access this data.");

            var members = (await _unitOfWork.Members.GetByBranchAsync(murabbi.BranchId))
                .Where(m => m.Role == UserRole.Member)
                .ToList();

            var result = new List<MemberProgressDto>();

            foreach (var member in members)
            {
                // Get total lessons scoped to THIS member's age group only
                var ageSections = await _unitOfWork.Sections
                    .GetByAgeGroupAsync(member.AgeGroup, publishedOnly: true);

                int totalLessons = 0;
                foreach (var section in ageSections)
                {
                    var units = await _unitOfWork.Units.GetBySectionIdAsync(section.Id, publishedOnly: true);
                    foreach (var unit in units)
                    {
                        var lessons = await _unitOfWork.Lessons.GetByUnitIdAsync(unit.Id);
                        totalLessons += lessons.Count();
                    }
                }

                var completed = await _unitOfWork.Progresses.CountCompletedByMemberAsync(member.Id);

                result.Add(new MemberProgressDto
                {
                    MemberId = member.Id,
                    FullName = $"{member.FirstName} {member.LastName}",
                    Email = member.Email,
                    AgeGroup = member.AgeGroup.ToString(),
                    TotalXp = member.TotalXp,
                    CurrentLevel = member.CurrentLevel,
                    CurrentStreak = member.Streak?.CurrentStreak ?? 0,
                    LessonsCompleted = completed,
                    ProgressPercentage = totalLessons > 0 ? (completed / (double)totalLessons) * 100 : 0,
                    LastActiveDate = member.LastActiveDate
                });
            }

            return result.OrderByDescending(m => m.TotalXp).ToList();
        }

        public async Task<BranchSummaryDto> GetBranchSummaryAsync(Guid murabbiId)
        {
            var murabbi = await _unitOfWork.Members.GetByIdAsync(murabbiId);
            if (murabbi == null || murabbi.Role != UserRole.Murabbi)
                throw new Exception("Unauthorized.");

            var members = (await _unitOfWork.Members.GetByBranchAsync(murabbi.BranchId))
                .Where(m => m.Role == UserRole.Member)
                .ToList();

            var weekAgo = DateTime.UtcNow.AddDays(-7);
            var topPerformers = new List<MemberProgressDto>();

            foreach (var m in members)
            {
                var ageSections = await _unitOfWork.Sections
                    .GetByAgeGroupAsync(m.AgeGroup, publishedOnly: true);

                int totalLessons = 0;
                foreach (var section in ageSections)
                {
                    var units = await _unitOfWork.Units.GetBySectionIdAsync(section.Id, publishedOnly: true);
                    foreach (var unit in units)
                    {
                        var lessons = await _unitOfWork.Lessons.GetByUnitIdAsync(unit.Id);
                        totalLessons += lessons.Count();
                    }
                }

                var completed = await _unitOfWork.Progresses.CountCompletedByMemberAsync(m.Id);

                topPerformers.Add(new MemberProgressDto
                {
                    MemberId = m.Id,
                    FullName = $"{m.FirstName} {m.LastName}",
                    Email = m.Email,
                    AgeGroup = m.AgeGroup.ToString(),
                    TotalXp = m.TotalXp,
                    CurrentLevel = m.CurrentLevel,
                    CurrentStreak = m.Streak?.CurrentStreak ?? 0,
                    LessonsCompleted = completed,
                    ProgressPercentage = totalLessons > 0 ? (completed / (double)totalLessons) * 100 : 0,
                    LastActiveDate = m.LastActiveDate
                });
            }

            topPerformers = topPerformers.OrderByDescending(m => m.TotalXp).Take(5).ToList();

            return new BranchSummaryDto
            {
                BranchId = murabbi.BranchId,
                BranchName = murabbi.Branch?.Name ?? string.Empty,
                TotalMembers = members.Count,
                ActiveMembersThisWeek = members.Count(m => m.LastActiveDate >= weekAgo),
                AverageProgress = topPerformers.Any() ? topPerformers.Average(m => m.ProgressPercentage) : 0,
                TopPerformers = topPerformers
            };
        }

        public async Task<MemberProgressDto?> GetMemberDetailsAsync(Guid murabbiId, Guid memberId)
        {
            var murabbi = await _unitOfWork.Members.GetByIdAsync(murabbiId);
            if (murabbi == null || murabbi.Role != UserRole.Murabbi)
                throw new Exception("Unauthorized.");

            var member = await _unitOfWork.Members.GetByIdWithDetailsAsync(memberId);
            if (member == null || member.BranchId != murabbi.BranchId)
                return null;

            var ageSections = await _unitOfWork.Sections
                .GetByAgeGroupAsync(member.AgeGroup, publishedOnly: true);

            int totalLessons = 0;
            foreach (var section in ageSections)
            {
                var units = await _unitOfWork.Units.GetBySectionIdAsync(section.Id, publishedOnly: true);
                foreach (var unit in units)
                {
                    var lessons = await _unitOfWork.Lessons.GetByUnitIdAsync(unit.Id);
                    totalLessons += lessons.Count();
                }
            }

            var completed = await _unitOfWork.Progresses.CountCompletedByMemberAsync(memberId);

            return new MemberProgressDto
            {
                MemberId = member.Id,
                FullName = $"{member.FirstName} {member.LastName}",
                Email = member.Email,
                AgeGroup = member.AgeGroup.ToString(),
                TotalXp = member.TotalXp,
                CurrentLevel = member.CurrentLevel,
                CurrentStreak = member.Streak?.CurrentStreak ?? 0,
                LessonsCompleted = completed,
                ProgressPercentage = totalLessons > 0 ? (completed / (double)totalLessons) * 100 : 0,
                LastActiveDate = member.LastActiveDate
            };
        }
    }
}
