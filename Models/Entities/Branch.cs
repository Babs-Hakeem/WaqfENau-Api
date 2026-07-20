namespace WaqfENau.Api.Models.Entities
{
    public class Branch : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;

        public ICollection<Member> Members { get; set; } = new List<Member>();
    }
}
