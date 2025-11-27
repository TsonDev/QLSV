using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class TeacherSubject
{
    public int Id { get; set; }

    public string TeacherId { get; set; } = null!;

    public string SubjectId { get; set; } = null!;

    public string? Status { get; set; }

    public virtual Subject Subject { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
