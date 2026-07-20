using Microsoft.AspNetCore.Identity;
using WaqfENau.Api.Infrastructure.Interfaces.Services;

namespace WaqfENau.Api.Infrastructure.Implementation.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly PasswordHasher<object> _passwordHasher = new();

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null!, password);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(null!, passwordHash, password);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
