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

        // ============================================================
        // 1) GET LIST BASIC
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetTeachers()
        {
            var data = await _context.Teachers
                .Where(t => t.Status == "Active")
                .Include(t => t.User)
                .Select(t => new {
                    TeacherId = t.TeacherId.Trim(),
                    Name = t.User != null ? t.User.Name.Trim() : null,
                    Email = t.User != null ? t.User.Email.Trim() : null,
                    Status = t.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // ============================================================
        // 2) GET DETAIL
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacher(string id)
        {
            var data = await _context.Teachers
                .Where(t => t.TeacherId.Trim() == id.Trim())
                .Include(t => t.User)
                .Select(t => new {
                    TeacherId = t.TeacherId.Trim(),
                    t.Status,
                    User = t.User == null ? null : new
                    {
                        t.User.Name,
                        t.User.Email,
                        t.User.PhoneNumber,
                        t.User.Birthday
                    }
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // ============================================================
        // 3) CREATE TEACHER
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> PostTeacher(TeacherCreateDto dto)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId))
                return BadRequest($"UserId {dto.UserId} không tồn tại.");

            // Generate ID
            var last = await _context.Teachers
                .OrderByDescending(t => t.TeacherId)
                .FirstOrDefaultAsync();

            int next = 1;
            if (last != null && last.TeacherId.StartsWith("Tch-"))
                next = int.Parse(last.TeacherId.Substring(4)) + 1;

            string newId = $"Tch-{next:D5}".PadRight(30);

            var teacher = new Teacher
            {
                TeacherId = newId,
                UserId = dto.UserId,
                Status = "Active"
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo giảng viên thành công", teacherId = newId.Trim() });
        }

        // ============================================================
        // 4) UPDATE TEACHER
        // ============================================================
        [HttpPut("full/{id}")]
        public async Task<IActionResult> UpdateTeacherFull(string id, TeacherUpdateDto dto)
        {
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherId.Trim() == id.Trim());

            if (teacher == null)
                return NotFound("Không tìm thấy giảng viên.");

            // ========== UPDATE USER ==========
            if (teacher.User != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Name))
                    teacher.User.Name = dto.Name;

                if (!string.IsNullOrWhiteSpace(dto.Email))
                    teacher.User.Email = dto.Email;

                if (dto.PhoneNumber != null)
                    teacher.User.PhoneNumber = dto.PhoneNumber;

                if (dto.Birthday != null)
                    teacher.User.Birthday = dto.Birthday;
            }

            // ========== UPDATE TEACHER ==========
            if (!string.IsNullOrWhiteSpace(dto.Status))
                teacher.Status = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật giảng viên thành công!" });
        }


        // ============================================================
        // 5) SOFT DELETE
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(string id)
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherId.Trim() == id.Trim());

            if (teacher == null)
                return NotFound();

            teacher.Status = "Inactive";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        // 6) RESTORE TEACHER
        // ============================================================
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreTeacher(string id)
        {
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.TeacherId.Trim() == id.Trim());

            if (teacher == null)
                return NotFound();

            teacher.Status = "Active";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ============================================================
        // 7) GET TEACHERS BY SUBJECT (FE dùng để chọn gv theo môn)
        // ============================================================
        [HttpGet("by-subject/{subjectId}")]
        public async Task<IActionResult> GetTeachersBySubject(string subjectId)
        {
            var teachers = await _context.TeacherSubjects
                .Where(ts => ts.SubjectId.Trim() == subjectId.Trim() && ts.Status == "Active")
                .Include(ts => ts.Teacher).ThenInclude(t => t.User)
                .Select(ts => new {
                    TeacherId = ts.Teacher.TeacherId.Trim(),
                    Name = ts.Teacher.User.Name.Trim(),
                    Email = ts.Teacher.User.Email.Trim()
                })
                .ToListAsync();

            return Ok(teachers);
        }

        // ============================================================
        // 8) ASSIGN TEACHER TO SUBJECT
        // ============================================================
        [HttpPost("assign-subject")]
        public async Task<IActionResult> AssignTeacherToSubject(AssignTeacherSubjectDto dto)
        {
            if (!await _context.Teachers.AnyAsync(t => t.TeacherId == dto.TeacherId))
                return BadRequest("TeacherId không tồn tại.");

            if (!await _context.Subjects.AnyAsync(s => s.Id == dto.SubjectId))
                return BadRequest("SubjectId không tồn tại.");

            bool exists = await _context.TeacherSubjects.AnyAsync(ts =>
                ts.TeacherId == dto.TeacherId &&
                ts.SubjectId == dto.SubjectId &&
                ts.Status == "Active"
            );

            if (exists)
                return BadRequest("Giảng viên đã được gán môn này.");

            var item = new TeacherSubject
            {
                TeacherId = dto.TeacherId,
                SubjectId = dto.SubjectId,
                Status = "Active"
            };

            _context.TeacherSubjects.Add(item);
            await _context.SaveChangesAsync();

            return Ok("Gán giáo viên dạy môn thành công!");
        }

        // ============================================================
        // 9) GET SUBJECT LIST OF TEACHER
        // ============================================================
        [HttpGet("{teacherId}/subjects")]
        public async Task<IActionResult> GetSubjectsOfTeacher(string teacherId)
        {
            var data = await _context.TeacherSubjects
                .Where(ts => ts.TeacherId.Trim() == teacherId.Trim() && ts.Status == "Active")
                .Include(ts => ts.Subject)
                .Select(ts => new {
                    SubjectId = ts.SubjectId.Trim(),
                    SubjectName = ts.Subject.Name
                })
                .ToListAsync();

            return Ok(data);
        }

        // ============================================================
        // 10) GET ALL CLASSES TAUGHT BY TEACHER
        // ============================================================
        [HttpGet("{teacherId}/classes")]
        public async Task<IActionResult> GetClassesOfTeacher(string teacherId)
        {
            var data = await _context.Classes
                .Where(c => c.TeacherId.Trim() == teacherId.Trim())
                .Select(c => new {
                    ClassId = c.ClassId.Trim(),
                    c.ClassName,
                    c.SemesterId,
                    c.DayOfWeek,
                    c.StartPeriod,
                    c.EndPeriod,
                    c.Room,
                    c.Type,
                    c.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // ============================================================
        // 11) GET CURRENT TEACHING CLASSES
        // ============================================================
        [HttpGet("{teacherId}/classes/current")]
        public async Task<IActionResult> GetCurrentClasses(string teacherId)
        {
            var data = await _context.Classes
                .Where(c => c.TeacherId.Trim() == teacherId.Trim() && c.Status == "Open")
                .Select(c => new {
                    ClassId = c.ClassId.Trim(),
                    c.ClassName,
                    c.Room,
                    c.DayOfWeek,
                    c.StartPeriod,
                    c.EndPeriod
                })
                .ToListAsync();

            return Ok(data);
        }

        // ============================================================
        // 12) GET TEACHING HISTORY (closed / finished)
        // ============================================================
        [HttpGet("{teacherId}/classes/history")]
        public async Task<IActionResult> GetTeachingHistory(string teacherId)
        {
            var data = await _context.Classes
                .Where(c => c.TeacherId.Trim() == teacherId.Trim() && c.Status != "Open")
                .Select(c => new {
                    ClassId = c.ClassId.Trim(),
                    c.ClassName,
                    c.SemesterId,
                    c.Type,
                    c.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // ============================================================
        // 13) WEEKLY TEACHING SCHEDULE
        // ============================================================
        [HttpGet("{teacherId}/schedule")]
        public async Task<IActionResult> GetWeeklySchedule(string teacherId)
        {
            var data = await _context.Classes
                .Where(c => c.TeacherId.Trim() == teacherId.Trim() && c.Status == "Open")
                .OrderBy(c => c.DayOfWeek)
                .ThenBy(c => c.StartPeriod)
                .Select(c => new {
                    c.DayOfWeek,
                    ClassId = c.ClassId.Trim(),
                    c.ClassName,
                    c.Room,
                    Time = $"{c.StartPeriod} - {c.EndPeriod}"
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
