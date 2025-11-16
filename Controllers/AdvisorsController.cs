using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;

namespace QLSV_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvisorsController : ControllerBase
    {
        private readonly QlsvContext _context;

        public AdvisorsController(QlsvContext context)
        {
            _context = context;
        }

        // GET: api/Advisors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Advisor>>> GetAdvisors()
        {
            return await _context.Advisors
                .Where(a => a.Status == "Active")
                .ToListAsync();
        }

        // GET: api/Advisors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Advisor>> GetAdvisor(string id)
        {
            var advisor = await _context.Advisors.FindAsync(id);

            if (advisor == null)
                return NotFound();

            if (advisor.Status == "Inactive")
                return BadRequest("Advisor is deleted.");

            return advisor;
        }

        // PUT: api/Advisors/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdvisor(string id, AdvisorUpdateDto dto)
        {
            var advisor = await _context.Advisors.FindAsync(id);
            if (advisor == null)
                return NotFound();

            if (advisor.Status == "Inactive")
                return BadRequest("Advisor is deleted.");

            // Update UserId
            if (!string.IsNullOrWhiteSpace(dto.UserId))
            {
                bool userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
                if (!userExists)
                    return BadRequest($"UserId {dto.UserId} không tồn tại.");

                advisor.UserId = dto.UserId;
            }

            // Update Status
            if (!string.IsNullOrWhiteSpace(dto.Status))
                advisor.Status = dto.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/Advisors
        [HttpPost]
        public async Task<ActionResult<Advisor>> PostAdvisor(AdvisorCreateDto dto)
        {
            // Kiểm tra UserId
            bool userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                return BadRequest($"UserId {dto.UserId} không tồn tại.");

            // Sinh ID mới theo format: Adv-00001
            var lastAdvisor = await _context.Advisors
                .Where(a => a.AdvisorId.StartsWith("Adv-"))
                .OrderByDescending(a => a.AdvisorId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastAdvisor != null)
            {
                string numberPart = lastAdvisor.AdvisorId.Substring(4);
                nextNumber = int.Parse(numberPart) + 1;
            }

            string newAdvisorId = $"Adv-{nextNumber:D5}";

            var advisor = new Advisor
            {
                AdvisorId = newAdvisorId,
                UserId = dto.UserId,
                Status = "Active"
            };

            _context.Advisors.Add(advisor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAdvisor), new { id = advisor.AdvisorId }, advisor);
        }

        // DELETE (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdvisor(string id)
        {
            var advisor = await _context.Advisors.FindAsync(id);
            if (advisor == null)
                return NotFound();

            advisor.Status = "Inactive";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // RESTORE
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreAdvisor(string id)
        {
            var advisor = await _context.Advisors.FindAsync(id);
            if (advisor == null)
                return NotFound();

            advisor.Status = "Active";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AdvisorExists(string id)
        {
            return _context.Advisors.Any(e => e.AdvisorId == id);
        }
    }
}
