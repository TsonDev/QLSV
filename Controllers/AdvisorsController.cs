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

        // =====================================================================
        // 1) GET LIST (JOIN USER) — ACTIVE ONLY
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> GetAdvisors()
        {
            var data = await _context.Advisors
                .Where(a => a.Status == "Active")
                .Include(a => a.User)
                .Select(a => new {
                    AdvisorId = a.AdvisorId.Trim(),
                    Name = a.User != null ? a.User.Name.Trim() : null,
                    Email = a.User != null ? a.User.Email.Trim() : null,
                    Status = a.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // =====================================================================
        // 2) GET DETAIL (JOIN USER)
        // =====================================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdvisor(string id)
        {
            var advisor = await _context.Advisors
                .Where(a => a.AdvisorId.Trim() == id.Trim())
                .Include(a => a.User)
                .Select(a => new {
                    AdvisorId = a.AdvisorId.Trim(),
                    a.Status,
                    User = a.User == null ? null : new
                    {
                        a.User.Name,
                        a.User.Email,
                        a.User.PhoneNumber,
                        a.User.Birthday
                    }
                })
                .FirstOrDefaultAsync();

            if (advisor == null)
                return NotFound("Không tìm thấy advisor.");

            return Ok(advisor);
        }

        // =====================================================================
        // 3) CREATE ADVISOR
        // =====================================================================
        [HttpPost]
        public async Task<IActionResult> PostAdvisor(AdvisorCreateDto dto)
        {
            // Validate UserId
            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
                return BadRequest($"UserId {dto.UserId} không tồn tại.");

            // Generate ID Adv-00001
            var last = await _context.Advisors
                .OrderByDescending(a => a.AdvisorId)
                .FirstOrDefaultAsync();

            int next = 1;
            if (last != null && last.AdvisorId.StartsWith("Adv-"))
                next = int.Parse(last.AdvisorId.Substring(4)) + 1;

            string newId = $"Adv-{next:D5}";

            var advisor = new Advisor
            {
                AdvisorId = newId,
                UserId = dto.UserId,
                Status = "Active"
            };

            _context.Advisors.Add(advisor);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo advisor thành công!", advisorId = newId });
        }

        // =====================================================================
        // 4) UPDATE (FULL)
        // =====================================================================
        [HttpPut("full/{id}")]
        public async Task<IActionResult> UpdateAdvisorFull(string id, AdvisorUpdateDto dto)
        {
            var advisor = await _context.Advisors
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AdvisorId.Trim() == id.Trim());

            if (advisor == null)
                return NotFound("Advisor không tồn tại.");

            // =========== UPDATE USER ===========
            if (advisor.User != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    advisor.User.Name = dto.Name;

                if (!string.IsNullOrWhiteSpace(dto.Email))
                    advisor.User.Email = dto.Email;

                if (dto.PhoneNumber != null)
                    advisor.User.PhoneNumber = dto.PhoneNumber;

                if (dto.Birthday != null)
                    advisor.User.Birthday = dto.Birthday;
            }

            // =========== UPDATE ADVISOR ===========
            if (!string.IsNullOrWhiteSpace(dto.Status))
                advisor.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật advisor thành công!" });
        }

        // =====================================================================
        // 5) SOFT DELETE
        // =====================================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdvisor(string id)
        {
            var advisor = await _context.Advisors
                .FirstOrDefaultAsync(a => a.AdvisorId.Trim() == id.Trim());

            if (advisor == null)
                return NotFound();

            advisor.Status = "Inactive";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =====================================================================
        // 6) RESTORE
        // =====================================================================
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreAdvisor(string id)
        {
            var advisor = await _context.Advisors
                .FirstOrDefaultAsync(a => a.AdvisorId.Trim() == id.Trim());

            if (advisor == null)
                return NotFound();

            advisor.Status = "Active";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =====================================================================
        // 7) ASSIGN ADVISOR TO STUDENT
        // =====================================================================
        [HttpPost("assign-student")]
        public async Task<IActionResult> AssignAdvisorToStudent(AssignAdvisorDto dto)
        {
            var advisor = await _context.Advisors
                .FirstOrDefaultAsync(a => a.AdvisorId.Trim() == dto.AdvisorId.Trim());

            if (advisor == null || advisor.Status == "Inactive")
                return BadRequest("AdvisorId không hợp lệ.");

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId.Trim() == dto.StudentId.Trim());

            if (student == null)
                return BadRequest("StudentId không tồn tại.");

            student.AdvisorId = dto.AdvisorId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Gán cố vấn cho sinh viên thành công!" });
        }

        // =====================================================================
        // 8) GET STUDENTS OF ADVISOR
        // =====================================================================
        [HttpGet("{advisorId}/students")]
        public async Task<IActionResult> GetStudentsOfAdvisor(string advisorId)
        {
            var students = await _context.Students
                .Where(s => s.AdvisorId.Trim() == advisorId.Trim() && s.Status == "Active")
                .Include(s => s.User)
                .Select(s => new {
                    StudentId = s.StudentId.Trim(),
                    Name = s.User != null ? s.User.Name : null,
                    Email = s.User != null ? s.User.Email : null,
                    PhoneNumber = s.User != null ? s.User.PhoneNumber : null,
                    Gender = s.User != null ? s.User.Gender : null,
                    Birthday = s.User != null ? s.User.Birthday : null,
                    
                })
                .ToListAsync();

            return Ok(students);
        }

    }
}
