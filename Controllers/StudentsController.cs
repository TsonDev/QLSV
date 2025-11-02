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
            return await _context.Students.ToListAsync();
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

            return student;
        }

        // PUT: api/Students/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(string id, Student student)
        {
            if (id != student.StudentId)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Students
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
       /* [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            _context.Students.Add(student);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StudentExists(student.StudentId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetStudent", new { id = student.StudentId }, student);
        }*/
      

        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent([FromBody] StudentCreateDto dto)
        {
            // Kiểm tra User có tồn tại chưa
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
            {
                return BadRequest(new { message = $"UserId {dto.UserId} không tồn tại." });
            }

            // Tạo Student mới
            var student = new Student
            {
                StudentId = dto.StudentId,
                UserId = dto.UserId,
                AdvisorId = dto.AdvisorId,
            };

            _context.Students.Add(student);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StudentExists(student.StudentId))
                {
                    return Conflict(new { message = $"StudentId {student.StudentId} đã tồn tại." });
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetStudent", new { id = student.StudentId }, student);
        }

        // DELETE: api/Students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Students.Remove(student);
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


    }
}
