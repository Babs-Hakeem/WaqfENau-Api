using WaqfENau.Api.DTOs;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Infrastructure.Interfaces.Services;
using WaqfENau.Api.Models.Entities;
using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Infrastructure.Implementation.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IIdentityService _identityService;

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IIdentityService identityService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _identityService = identityService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _unitOfWork.Members.AnyAsync(m => m.Email == request.Email))
                throw new Exception("Email already registered");

            var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.BranchId)
                ?? throw new Exception("Branch not found");

            var age = DateTime.Today.Year - request.DateOfBirth.Year;
            if (request.DateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

            var ageGroup = age switch
            {
                <= 9  => AgeGroup.Children7_9,
                <= 12 => AgeGroup.Children10_12,
                <= 15 => AgeGroup.Teenagers13_15,
                _     => AgeGroup.Youth16_Plus
            };

            var member = new Member
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = _identityService.HashPassword(request.Password),
                DateOfBirth = request.DateOfBirth,
                AgeGroup = ageGroup,
                BranchId = request.BranchId,
                Role = UserRole.Member,
                DailyGoalMinutes = 10
            };

            // Create streak and hearts alongside the member
            var streak = new Streak { MemberId = member.Id };
            var hearts = new Hearts { MemberId = member.Id, Current = Hearts.Max };

            await _unitOfWork.Members.AddAsync(member);
            await _unitOfWork.Streaks.AddAsync(streak);
            await _unitOfWork.Repository<Hearts>().AddAsync(hearts);
            await _unitOfWork.SaveChangesAsync();

            var accessToken = _jwtService.GenerateAccessToken(member);
            var refreshToken = _jwtService.GenerateRefreshToken();
            await _jwtService.SaveRefreshTokenAsync(member.Id, refreshToken);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Member = MapToMemberDto(member, branch.Name)
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var member = await _unitOfWork.Members.GetByIdWithDetailsAsync(
                (await _unitOfWork.Members.GetByEmailAsync(request.Email))?.Id ?? Guid.Empty)
                ?? throw new Exception("Invalid email or password");

            if (!_identityService.VerifyPassword(request.Password, member.PasswordHash))
                throw new Exception("Invalid email or password");

            member.LastActiveDate = DateTime.UtcNow;
            _unitOfWork.Members.Update(member);
            await _unitOfWork.SaveChangesAsync();

            var accessToken = _jwtService.GenerateAccessToken(member);
            var refreshToken = _jwtService.GenerateRefreshToken();
            await _jwtService.SaveRefreshTokenAsync(member.Id, refreshToken);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Member = MapToMemberDto(member, member.Branch?.Name ?? string.Empty)
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var member = await _jwtService.ValidateRefreshTokenAsync(request.RefreshToken)
                ?? throw new Exception("Invalid refresh token");

            await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken);

            var fullMember = await _unitOfWork.Members.GetByIdWithDetailsAsync(member.Id) ?? member;

            var accessToken = _jwtService.GenerateAccessToken(fullMember);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            await _jwtService.SaveRefreshTokenAsync(fullMember.Id, newRefreshToken);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                Member = MapToMemberDto(fullMember, fullMember.Branch?.Name ?? string.Empty)
            };
        }

        public async Task LogoutAsync(Guid memberId)
        {
            var tokens = await _unitOfWork.Repository<RefreshToken>()
                .FindAsync(rt => rt.MemberId == memberId && !rt.IsRevoked);

            foreach (var token in tokens)
                token.IsRevoked = true;

            await _unitOfWork.SaveChangesAsync();
        }

        private static MemberDto MapToMemberDto(Member member, string branchName) => new()
        {
            Id = member.Id,
            FirstName = member.FirstName,
            LastName = member.LastName,
            Email = member.Email,
            AgeGroup = member.AgeGroup.ToString(),
            Role = member.Role.ToString(),
            TotalXp = member.TotalXp,
            CurrentLevel = member.CurrentLevel,
            BranchName = branchName
        };
    }
}
