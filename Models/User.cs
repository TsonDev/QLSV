using System;
using System.Collections.Generic;

namespace QLSV_V1.Models;

public partial class User
{
    public string Id { get; set; } = null!;

    public string? Name { get; set; }

    public string? Email { get; set; }

    public DateOnly? Birthday { get; set; }

    public string? Gender { get; set; }

    public int? PhoneNumber { get; set; }

    public string? AccId { get; set; }

    public string? AddId { get; set; }

    public virtual Account? Acc { get; set; }

    public virtual Address? Add { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
