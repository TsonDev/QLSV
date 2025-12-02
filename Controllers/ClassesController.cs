using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;

namespace QLSV_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  
    public class ClassesController : ControllerBase
    {
        private readonly QlsvContext _context;

        public ClassesController(QlsvContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET ALL CLASSES (Public)
        // =====================================================
        [HttpGet]
        [AllowAnonymous]  
        public async Task<IActionResult> GetClasses()
        {
            var data = await _context.Classes
                .Where(c => c.Status == "Open")
                .Select(c => new
                {
                    ClassId = c.ClassId.Trim(),
                    ClassName = c.ClassName.Trim(),
                    SubjectId = c.SubjectId.Trim(),
                    SubjectName = _context.Subjects
                        .Where(s => s.Id.Trim() == c.SubjectId.Trim())
                        .Select(s => s.Name)
                        .FirstOrDefault(),

                    TeacherId = c.TeacherId.Trim(),
                    TeacherName = _context.Teachers
                        .Where(t => t.TeacherId.Trim() == c.TeacherId.Trim())
                        .Include(t => t.User)
                        .Select(t => t.User.Name.Trim())
                        .FirstOrDefault(),

                    SemesterId = c.SemesterId.Trim(),
                    DayOfWeek = c.DayOfWeek,
                    StartPeriod = c.StartPeriod,
                    EndPeriod = c.EndPeriod,
                    Room = c.Room,
                    Type = c.Type,
                    MaxStudents = c.MaxStudents,
                    CurrentStudents = c.CurrentStudents,
                    Status = c.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // =====================================================
        // GET DETAIL (Public)
        // =====================================================
        [HttpGet("{classId}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetClass(string classId)
        {
            var c = await _context.Classes
                .Where(x => x.ClassId.Trim() == classId.Trim())
                .FirstOrDefaultAsync();

            if (c == null)
                return NotFound();

            return Ok(c);
        }

        // =====================================================
        // CHECK TEACHER SCHEDULE CONFLICT
        // =====================================================
        private async Task<bool> IsTeacherScheduleConflict(string teacherId, byte day, byte start, byte end)
        {
            return await _context.Classes.AnyAsync(c =>
                c.TeacherId == teacherId &&
                c.DayOfWeek == day &&
                c.StartPeriod <= end &&
                c.EndPeriod >= start &&
                c.Status == "Open"
            );
        }

        // =====================================================
        // CREATE CLASS  (Admin / Teacher)
        // =====================================================
        [HttpPost("create")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateClass(ClassCreateDto dto)
        {
            if (!await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
                return BadRequest("Subject không tồn tại.");

            if (!await _context.Teachers.AnyAsync(t => t.TeacherId == dto.TeacherId))
                return BadRequest("Teacher không tồn tại.");

            if (await IsTeacherScheduleConflict(dto.TeacherId, dto.DayOfWeek, dto.StartPeriod, dto.EndPeriod))
                return BadRequest("Giáo viên bị trùng lịch.");

            var last = await _context.Classes
                .OrderByDescending(c => c.ClassId)
                .FirstOrDefaultAsync();

            int next = 1;
            if (last != null && last.ClassId.StartsWith("CLS-"))
                next = int.Parse(last.ClassId.Substring(4)) + 1;

            string newClassId = $"CLS-{next:D5}".PadRight(30);

            var entity = new Class
            {
                ClassId = newClassId,
                ClassName = dto.ClassName,
                SubjectId = dto.SubjectId,
                SemesterId = dto.SemesterId,
                TeacherId = dto.TeacherId,
                MaxStudents = dto.MaxStudents,
                CurrentStudents = 0,
                DayOfWeek = dto.DayOfWeek,
                StartPeriod = dto.StartPeriod,
                EndPeriod = dto.EndPeriod,
                Room = dto.Room,
                Type = dto.Type,
                Note = dto.Note,
                Status = "Open",
                DateCreate = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Classes.Add(entity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tạo lớp học phần thành công!",
                classId = newClassId.Trim()
            });
        }

        // =====================================================
        // UPDATE CLASS  (Admin / Teacher)
        // =====================================================
        [HttpPut("update/{classId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateClass(string classId, ClassUpdateDto dto)
        {
            var entity = await _context.Classes
                .FirstOrDefaultAsync(c => c.ClassId.Trim() == classId.Trim());

            if (entity == null)
                return NotFound("Class không tồn tại.");

            if (!await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
                return BadRequest("Subject không tồn tại.");

            if (!await _context.Teachers.AnyAsync(t => t.TeacherId == dto.TeacherId))
                return BadRequest("Teacher không tồn tại.");

            bool conflict = await _context.Classes.AnyAsync(c =>
                c.TeacherId == dto.TeacherId &&
                c.DayOfWeek == dto.DayOfWeek &&
                c.StartPeriod <= dto.EndPeriod &&
                c.EndPeriod >= dto.StartPeriod &&
                c.ClassId != entity.ClassId &&
                c.Status == "Open"
            );

            if (conflict)
                return BadRequest("Giáo viên bị trùng lịch.");

            entity.ClassName = dto.ClassName;
            entity.SubjectId = dto.SubjectId;
            entity.SemesterId = dto.SemesterId;
            entity.TeacherId = dto.TeacherId;
            entity.MaxStudents = dto.MaxStudents;
            entity.DayOfWeek = dto.DayOfWeek;
            entity.StartPeriod = dto.StartPeriod;
            entity.EndPeriod = dto.EndPeriod;
            entity.Room = dto.Room;
            entity.Type = dto.Type;
            entity.Note = dto.Note;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật lớp thành công!" });
        }

        // =====================================================
        // SOFT DELETE  (Admin)
        // =====================================================
        [HttpDelete("soft/{classId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDelete(string classId)
        {
            var c = await _context.Classes
                .FirstOrDefaultAsync(x => x.ClassId.Trim() == classId.Trim());

            if (c == null)
                return NotFound();

            c.Status = "InOpen";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =====================================================
        // RESTORE (Admin)
        // =====================================================
        [HttpPut("restore/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var c = await _context.Classes.FindAsync(id);
            if (c == null) return NotFound();

            c.Status = "Active";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }
    }
}
