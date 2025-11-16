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
    public class TeachersController : ControllerBase
    {
        private readonly QlsvContext _context;

        public TeachersController(QlsvContext context)
        {
            _context = context;
        }

        // GET: api/Teachers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachers()
        {
            return await _context.Teachers.Where(t=>t.Status=="Active").ToListAsync();
        }

        // GET: api/Teachers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Teacher>> GetTeacher(string id)
        {
            var teacher = await _context.Teachers.FindAsync(id);

            if (teacher == null)
            {
                return NotFound();
            }
            if (teacher.Status == "Inactive")
                return BadRequest("Teacher is deleted.");
            return teacher;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTeacher(string id, TeacherUpdateDto dto)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound();

            if (teacher.Status == "Inactive")
                return BadRequest("Teacher is deleted.");

            // Nếu FE sửa UserId thì kiểm tra tài khoản User có tồn tại hay không
            if (!string.IsNullOrWhiteSpace(dto.UserId))
            {
                bool userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
                if (!userExists)
                    return BadRequest($"UserId {dto.UserId} không tồn tại.");

                teacher.UserId = dto.UserId;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/Teachers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Teacher>> PostTeacher(TeacherCreateDto dto)
        {
            // Kiểm tra UserId có tồn tại
            bool userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                return BadRequest($"UserId {dto.UserId} không tồn tại.");

            // Tìm teacher có format mới
            var lastTeacher = await _context.Teachers
                .Where(t => t.TeacherId.StartsWith("Tch-"))
                .OrderByDescending(t => t.TeacherId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastTeacher != null)
            {
                string numberPart = lastTeacher.TeacherId.Substring(4);
                nextNumber = int.Parse(numberPart) + 1;
            }

            string newTeacherId = $"Tch-{nextNumber:D5}";

            var teacher = new Teacher
            {
                TeacherId = newTeacherId,
                UserId = dto.UserId,
                Status = "Active"
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.TeacherId }, teacher);
        }


        // DELETE: api/Teachers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(string id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            teacher.Status = "Inactive";
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreTeacher(string id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound();

            teacher.Status = "Active";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TeacherExists(string id)
        {
            return _context.Teachers.Any(e => e.TeacherId == id);
        }
    }
}
