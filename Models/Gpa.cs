using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class Gpa
{
    public string Id { get; set; } = null!;

    public double? Gpa1 { get; set; }

    public int? TongTc { get; set; }

    public string? Semesterid { get; set; }

    public string? Studentid { get; set; }

    public virtual Semester? Semester { get; set; }

    public virtual Student? Student { get; set; }
}
