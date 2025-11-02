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
}
