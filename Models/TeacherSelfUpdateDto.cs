namespace QLSV_V1.Models
{
    public class TeacherSelfUpdateDto
    {
        public string AccId { get; set; }     // bắt buộc (user ID)
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? PhoneNumber { get; set; }
        public DateOnly? Birthday { get; set; }
        public string? Gender { get; set; }
    }
}
