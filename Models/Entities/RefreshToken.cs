namespace WaqfENau.Api.Models.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public string? ReplacedByToken { get; set; }
    }
}
