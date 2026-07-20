using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(Member member);
        string GenerateRefreshToken();
        Task<RefreshToken> SaveRefreshTokenAsync(Guid memberId, string token);
        Task<Member?> ValidateRefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }
}
