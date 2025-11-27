using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
    [Column("ClassID")]
    public int? ClassId { get; set; }

    public int? SoTiet { get; set; }

    public int? SoTietNghi { get; set; }

    public int? IsApproved { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual Semester Semester { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
    public virtual Class Class { get; set; } = null!;
}
