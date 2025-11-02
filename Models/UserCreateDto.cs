namespace QLSV_V1.Models
{
    public class UserCreateDto
    {
        public string Id { get; set; } = null!;

        public string? Name { get; set; }

        public string? Email { get; set; }

        public DateOnly? Birthday { get; set; }

        public string? Gender { get; set; }

        public int? PhoneNumber { get; set; }

        public string? AccId { get; set; }

        public AddressCreateDto Address { get; set; } = new AddressCreateDto();

    }
}
