using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class Class
{
    public int Id { get; set; }

    public string? ClassName { get; set; }

    public string? SubjectId { get; set; }

    public string? SemesterId { get; set; }

    public string? TeacherId { get; set; }

    public DateOnly? DateCreate { get; set; }

    public string? Status { get; set; }

    public string? ClassId { get; set; }

    public int? MaxStudents { get; set; }

    public int? CurrentStudents { get; set; }

    public string? Note { get; set; }

    public string? Room { get; set; }

    public string? Schedule { get; set; }

    public string? Type { get; set; }

    public byte? DayOfWeek { get; set; }

    public byte? StartPeriod { get; set; }

    public byte? EndPeriod { get; set; }
    public virtual Subject Subject { get; set; }
}
