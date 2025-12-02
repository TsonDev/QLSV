namespace QLSV_V1.Models
{
    public class StudentUpdateInfoDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? PhoneNumber { get; set; }
        public DateOnly? Birthday { get; set; }
        public string? Province { get; set; } // example address field if you want
        public string? AdvisorId { get; set; }
        public string? Status { get; set; }
    }
}
