namespace QLSV_V1.Models
{
    public class AdvisorSelfUpdateDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? PhoneNumber { get; set; }
        public DateOnly? Birthday { get; set; }
        public string? Gender { get; set; }
    }
}
