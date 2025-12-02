namespace QLSV_V1.Models
{
    public class ClassUpdateDto
    {
        public string ClassName { get; set; } = null!;

        public string SubjectId { get; set; } = null!;

        public string SemesterId { get; set; } = null!;

        public string TeacherId { get; set; } = null!;

        public byte DayOfWeek { get; set; }

        public byte StartPeriod { get; set; }

        public byte EndPeriod { get; set; }

        public string Room { get; set; } = null!;

        public string Type { get; set; } = null!;

        public int MaxStudents { get; set; }

        public string? Note { get; set; }
    }
}
