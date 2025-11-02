using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class StudentSubject
{
    public string StudentId { get; set; } = null!;

    public string SubjectId { get; set; } = null!;

    public string SemesterId { get; set; } = null!;

    public double? Point1 { get; set; }

    public double? Point2 { get; set; }

    public double? Point3 { get; set; }

    public double? PointTotal { get; set; }

    public string? Status { get; set; }

    public int? ClassId { get; set; }

    public virtual Semester Semester { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
