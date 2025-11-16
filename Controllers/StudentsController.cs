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
    public class StudentsController : ControllerBase
    {
        private readonly QlsvContext _context;

        public StudentsController(QlsvContext context)
        {
            _context = context;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students.Where(a=>a.Status=="Active").ToListAsync();
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(string id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }
            if (student.Status == "Inactive")
                return BadRequest("Student is deleted.");

            return student;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(string id, StudentUpdateDto dto)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            if (student.Status == "Inactive")
                return BadRequest("Student is deleted.");

            if (!string.IsNullOrWhiteSpace(dto.UserId))
                student.UserId = dto.UserId;

            if (!string.IsNullOrWhiteSpace(dto.AdvisorId))
                student.AdvisorId = dto.AdvisorId;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                student.Status = dto.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(StudentCreateDto dto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                return BadRequest(new { message = $"UserId {dto.UserId} không tồn tại." });

            // Tự sinh StudentId
            var lastStudent = await _context.Students
                .OrderByDescending(s => s.StudentId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastStudent != null)
            {
                string numberPart = lastStudent.StudentId.Substring(4);
                nextNumber = int.Parse(numberPart) + 1;
            }

            string newStudentId = $"Stu-{nextNumber.ToString("D5")}";

            var student = new Student
            {
                StudentId = newStudentId,
                UserId = dto.UserId,
                AdvisorId = dto.AdvisorId,
                Status = "Active"
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudent", new { id = student.StudentId }, student);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();

            student.Status = "Inactive";
            await _context.SaveChangesAsync();

            return NoContent();
        }


        private bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.StudentId == id);
        }
        [HttpGet("full")]
        public async Task<IActionResult> GetStudentsFull()
        {
            var data = await _context.Students
                .Where(s=>s.Status=="Active")
                .Include(s => s.User)
                .Include(s => s.Gpas)
                .Include(s => s.Conducts)
                .Select(s => new {
                    Id = s.StudentId.Trim(),
                    Name = s.User != null ? s.User.Name : null,
                    Email = s.User != null ? s.User.Email : null,
                    RecentGPAs = s.Gpas
            .OrderByDescending(g => g.Semesterid)
            .Take(3)
            .Select(g => new {
                g.Semesterid,
                GPA = g.Gpa1
            })
            .ToList(),
                    Conduct = s.Conducts.OrderByDescending(c => c.SemesterId).Select(c => c.Point).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(data);
        }
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreStudent(string id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.Status = "Active";
            await _context.SaveChangesAsync();

            return NoContent();
        }



    }
}
