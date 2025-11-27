namespace QLSV_V1.Models
{
    public class TeacherUpdateDto
    {
        public string UserId { get; set; }
        public string Status {  get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? PhoneNumber { get; set; }
        public DateOnly? Birthday { get; set; }

    }
}
