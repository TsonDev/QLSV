using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ResetTicketsController : ControllerBase
{
    private readonly QlsvContext _context;

    public ResetTicketsController(QlsvContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _context.ResetTickets
            .OrderByDescending(t => t.RequestedAt)
            .Select(t => new
            {
                t.Id,
                AccId = t.AccId ?? "",
                Username = t.Username ?? "",
                Status = t.Status ?? "Pending",
                RequestedAt = t.RequestedAt,
                ProcessedAt = t.ProcessedAt,
            })
            .ToListAsync();

        return Ok(data);
    }

}
