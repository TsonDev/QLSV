using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class Student
{
    public string StudentId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? AdvisorId { get; set; }

    public virtual ICollection<Conduct> Conducts { get; set; } = new List<Conduct>();

    public virtual ICollection<Gpa> Gpas { get; set; } = new List<Gpa>();

    public virtual ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();

    public virtual User? User { get; set; }
}
