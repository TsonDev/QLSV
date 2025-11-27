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
    public class SubjectsController : ControllerBase
    {
        private readonly QlsvContext _context;

        public SubjectsController(QlsvContext context)
        {
            _context = context;
        }

        // GET LIST
        [HttpGet]
        public async Task<IActionResult> GetSubjects()
        {
            var data = await _context.Subjects
                .Where(s => s.Status == "Active")
                .Select(s => new {
                    Id = s.Id.Trim(),
                    s.Name,
                    s.Type,
                    SoTc = s.SoTc,
                    s.CurriculumTerm,
                    s.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // GET DETAIL
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubject(string id)
        {
            var subject = await _context.Subjects
                .Where(s => s.Id.Trim() == id.Trim())
                .Select(s => new {
                    Id = s.Id.Trim(),
                    s.Name,
                    s.Type,
                    SoTc = s.SoTc,
                    s.CurriculumTerm,
                    s.Status
                })
                .FirstOrDefaultAsync();

            if (subject == null)
                return NotFound();

            return Ok(subject);
        }

        // AUTO GENERATE SubjectId
        private async Task<string> GenerateSubjectId()
        {
            var ids = await _context.Subjects
                .Select(s => s.Id.Trim())
                .ToListAsync();

            int max = 0;

            foreach (var id in ids)
            {
                if (id.StartsWith("Sub-") && int.TryParse(id.Substring(4), out int num))
                {
                    if (num > max)
                        max = num;
                }
            }

            return $"Sub-{(max + 1):D5}";
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> PostSubject(SubjectCreateDto dto)
        {
            // Kiểm tra trùng tên môn
            bool nameExists = await _context.Subjects
                .AnyAsync(s => s.Name == dto.Name);
            if (nameExists)
                return BadRequest("Tên môn đã tồn tại.");

            string newId = await GenerateSubjectId();
            string finalId = newId.PadRight(30);

            var subject = new Subject
            {
                Id = finalId,
                Name = dto.Name,
                Type = dto.Type,
                SoTc = dto.SoTc,
                CurriculumTerm = dto.CurriculumTerm,
                Status = "Active"
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo môn học thành công", subjectId = newId });
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubject(string id, SubjectUpdateDto dto)
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id.Trim() == id.Trim());

            if (subject == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Name))
                subject.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Type))
                subject.Type = dto.Type;

            if (dto.SoTc != null)
                subject.SoTc = dto.SoTc;

            if (dto.CurriculumTerm != null)
                subject.CurriculumTerm = dto.CurriculumTerm;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                subject.Status = dto.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE (SOFT)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(string id)
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id.Trim() == id.Trim());

            if (subject == null)
                return NotFound();

            subject.Status = "Inactive";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // RESTORE
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreSubject(string id)
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(s => s.Id.Trim() == id.Trim());

            if (subject == null)
                return NotFound();

            subject.Status = "Active";
            await _context.SaveChangesAsync();

            return NoContent();
        }
        //Lấy danh sách giảng viên theo môn
        [HttpGet("by-teacher/{subjectId}")]
        public async Task<IActionResult> GetTeachersBySubject(string subjectId)
        {
            var teachers = await _context.TeacherSubjects
                .Where(ts => ts.SubjectId.Trim() == subjectId.Trim() && ts.Status == "Active")
                .Include(ts => ts.Teacher)
                .ThenInclude(t => t.User)
                .Select(ts => new {
                    TeacherId = ts.Teacher.TeacherId.Trim(),
                    Name = ts.Teacher.User.Name.Trim(),
                    Email = ts.Teacher.User.Email.Trim()
                })
                .ToListAsync();

            return Ok(teachers);
        }

    }

}
