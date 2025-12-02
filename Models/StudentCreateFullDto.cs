namespace QLSV_V1.Models
{
    public class StudentCreateFullDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly? Birthday { get; set; }
        public string? AdvisorId { get; set; }
        public bool CreateAccount { get; set; } = true;
        public string? Username { get; set; }    // optional: if not provided we generate from user id
        public string? Password { get; set; }
    }
}
