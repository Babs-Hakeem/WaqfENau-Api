namespace WaqfENau.Api.Infrastructure.Interfaces.Services
{
    public interface IIdentityService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}
