using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class Account
{
    public string AccId { get; set; } = null!;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public DateOnly? DateCreate { get; set; }

    public string? CreateBy { get; set; }

    public string? Status { get; set; }

    public string? Role { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
