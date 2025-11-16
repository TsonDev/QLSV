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

        // GET: api/Subjects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subject>>> GetSubjects()
        {
            return await _context.Subjects
                .Where(s=>s.Status=="Active").ToListAsync();
        }

        // GET: api/Subjects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Subject>> GetSubject(string id)
        {
            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
            {
                return NotFound();
            }
            if (subject.Status == "Inactive")
                return BadRequest("Subject is deleted.");
            return subject;
        }

        // PUT: api/Subjects/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubject(string id, SubjectUpdateDto dto)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
                return NotFound();

            if (subject.Status == "Inactive")
                return BadRequest("Subject is deleted.");

            if (!string.IsNullOrWhiteSpace(dto.Type))
                subject.Type = dto.Type;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                subject.Name = dto.Name;

            if (dto.SoTc != null)
                subject.SoTc = dto.SoTc;

            if (dto.CurriculumTerm != null)
                subject.CurriculumTerm = dto.CurriculumTerm;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        // POST: api/Subjects
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Subject>> PostSubject(SubjectCreateDto dto)
        {
            // Lấy Subject cuối cùng
            var lastSubject = await _context.Subjects
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastSubject != null && lastSubject.Id.StartsWith("Sub-"))
            {
                string numberPart = lastSubject.Id.Substring(4);
                nextNumber = int.Parse(numberPart) + 1;
            }

            string newSubjectId = $"Sub-{nextNumber:D5}";

            var subject = new Subject
            {
                Id = newSubjectId,
                Type = dto.Type,
                Name = dto.Name,
                SoTc = dto.SoTc,
                CurriculumTerm = dto.CurriculumTerm,
                Status = "Active"
            };

            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubject), new { id = subject.Id }, subject);
        }




        // DELETE: api/Subjects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(string id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            subject.Status = "Inactive";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SubjectExists(string id)
        {
            return _context.Subjects.Any(e => e.Id == id);
        }
    }
}
