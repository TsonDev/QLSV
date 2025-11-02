using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class Conduct
{
    public string Id { get; set; } = null!;

    public int? Point { get; set; }

    public string? SemesterId { get; set; }

    public string? StudentId { get; set; }

    public virtual Semester? Semester { get; set; }

    public virtual Student? Student { get; set; }
}
