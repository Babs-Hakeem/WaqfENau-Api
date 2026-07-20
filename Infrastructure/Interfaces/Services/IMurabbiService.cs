using WaqfENau.Api.DTOs;

namespace WaqfENau.Api.Infrastructure.Interfaces.Services
{
    public interface IMurabbiService
    {
        Task<List<MemberProgressDto>> GetBranchMembersAsync(Guid murabbiId);
        Task<BranchSummaryDto> GetBranchSummaryAsync(Guid murabbiId);
        Task<MemberProgressDto?> GetMemberDetailsAsync(Guid murabbiId, Guid memberId);
    }
}
