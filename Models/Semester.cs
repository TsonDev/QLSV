using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class Semester
{
    public string SemesterId { get; set; } = null!;

    public string? Name { get; set; }

    public string? Year { get; set; }

    public virtual ICollection<Conduct> Conducts { get; set; } = new List<Conduct>();

    public virtual ICollection<Gpa> Gpas { get; set; } = new List<Gpa>();

    public virtual ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
}
