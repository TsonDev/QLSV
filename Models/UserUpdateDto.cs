namespace QLSV_V1.Models
{
    public class UserUpdateDto
    {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public DateOnly? Birthday { get; set; }
            public string? Gender { get; set; }
            public int? PhoneNumber { get; set; }

            public AddressUpdateDto? Address { get; set; }
    }
}
