using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class Advisor
{
    public string AdvisorId { get; set; } = null!;

    public string? UserId { get; set; }
}
