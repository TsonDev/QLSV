namespace QLSV_V1.Models
{
    public class StudentUpdateDto
    {
        public string? UserId { get; set; }

        public string? AdvisorId { get; set; }

        public string Status { get; set; } = null!;
    }
}
