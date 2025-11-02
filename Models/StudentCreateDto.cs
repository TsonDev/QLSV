namespace QLSV_V1.Models
{
    public class StudentCreateDto
    {
        public string StudentId { get; set; } = null!;

        public string? UserId { get; set; }

        public string? AdvisorId { get; set; }

    }
}
