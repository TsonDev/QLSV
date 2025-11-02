using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class Teacher
{
    public string TeacherId { get; set; } = null!;

    public string? UserId { get; set; }
}
