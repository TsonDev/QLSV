using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class Subject
{
    public string Id { get; set; } = null!;

    public string? Type { get; set; }

    public string? Name { get; set; }

    public int? SoTc { get; set; }

    public int? CurriculumTerm { get; set; }

    public virtual ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
}
