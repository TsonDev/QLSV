namespace QLSV_V1.Models
{
    public class ClassCreateDto
    {
        public string ClassName { get; set; }
        public string SubjectId { get; set; }
        public string SemesterId { get; set; }
        public string TeacherId { get; set; }

        public int MaxStudents { get; set; }

        public byte DayOfWeek { get; set; }
        public byte StartPeriod { get; set; }
        public byte EndPeriod { get; set; }

        public string? Room { get; set; }
        public string? Type { get; set; }
        public string? Note { get; set; }
    }
}
