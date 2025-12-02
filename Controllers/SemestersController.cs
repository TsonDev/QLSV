using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;

namespace QLSV_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SemestersController : ControllerBase
    {
        private readonly QlsvContext _context;

        public SemestersController(QlsvContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. GET ALL SEMESTERS
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Semesters
                .Select(s => new
                {
                    SemesterId = s.SemesterId.Trim(),
                    Name = s.Name!.Trim(),
                    Year = s.Year!.Trim()
                })
                .ToListAsync();

            return Ok(data);
        }

        // ============================================================
        // 2. GET SEMESTER DETAIL
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var semester = await _context.Semesters
                .FirstOrDefaultAsync(s => s.SemesterId.Trim() == id.Trim());

            if (semester == null)
                return NotFound("Không tìm thấy học kỳ.");

            return Ok(new
            {
                SemesterId = semester.SemesterId.Trim(),
                Name = semester.Name?.Trim(),
                Year = semester.Year?.Trim()
            });
        }

        // ============================================================
        // 3. CREATE SEMESTER
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Create(Semester dto)
        {
            // Kiểm tra trùng
            if (await _context.Semesters.AnyAsync(s => s.SemesterId.Trim() == dto.SemesterId.Trim()))
                return Conflict("SemesterId đã tồn tại.");

            _context.Semesters.Add(dto);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tạo học kỳ thành công.",
                semesterId = dto.SemesterId.Trim()
            });
        }

        // ============================================================
        // 4. UPDATE SEMESTER
        // ============================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Semester dto)
        {
            var semester = await _context.Semesters
                .FirstOrDefaultAsync(s => s.SemesterId.Trim() == id.Trim());

            if (semester == null)
                return NotFound("Không tìm thấy học kỳ.");

            semester.Name = dto.Name;
            semester.Year = dto.Year;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật học kỳ thành công." });
        }

        // ============================================================
        // 5. DELETE SEMESTER
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var semester = await _context.Semesters
                .FirstOrDefaultAsync(s => s.SemesterId.Trim() == id.Trim());

            if (semester == null)
                return NotFound("Không tìm thấy học kỳ.");

            _context.Semesters.Remove(semester);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa học kỳ thành công." });
        }
    }
}
