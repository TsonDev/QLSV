namespace QLSV_V1.Models
{
    public class AccountDto
    {
        public string AccId { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public DateOnly? DateCreate { get; set; }
    }
}
