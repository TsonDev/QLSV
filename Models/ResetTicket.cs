namespace QLSV_V1.Models
{
    public class ResetTicket
    {
        public int Id { get; set; }
        public string? AccId { get; set; }
        public string? Username { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedBy { get; set; }
    }
}
