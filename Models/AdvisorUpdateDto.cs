namespace QLSV_V1.Models
{
    public class AdvisorUpdateDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateOnly? Birthday { get; set; }
        public int? PhoneNumber { get; set; }

        // UPDATE Advisor
        public string? UserId { get; set; }
        public string? Status { get; set; }
    }
}