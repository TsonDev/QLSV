using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class Address
{
    public string AddId { get; set; } = null!;

    public string? Province { get; set; }

    public string? District { get; set; }

    public string? Infor { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
