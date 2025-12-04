using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;

namespace QLSV_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]   
    public class StatisticsController : ControllerBase
    {
        private readonly QlsvContext _context;

        public StatisticsController(QlsvContext context)
        {
            _context = context;
        }

        // ============================================================
        // 2. THỐNG KÊ SINH VIÊN THEO KỲ (Semester)
        // ============================================================
        [HttpGet("students/by-semester/{semesterId}")]
        public async Task<IActionResult> GetStudentsByCourse(string semesterId)
        {
            var total = await _context.Semesters
                .Where(se => se.SemesterId == semesterId)
                .SelectMany(se => se.StudentSubjects)
                .Select(ss => ss.StudentId)
                .Distinct()
                .CountAsync();

            return Ok(new
            {
                semesterId,
                total
            });
        }

        // ============================================================
        // 3. TỶ LỆ ĐẬU – RỚT THEO MÔN
        // ============================================================
        [HttpGet("pass-fail/{subjectId}")]
        public async Task<IActionResult> GetPassFailRate(string subjectId)
        {
            var scores = await _context.StudentSubjects
                .Where(ss => ss.SubjectId.Trim() == subjectId.Trim())
                .Select(ss => ss.Status)
                .ToListAsync();

            if (scores.Count == 0)
                return Ok(new { subjectId, total = 0, pass = 0, fail = 0, passRate = 0, failRate = 0 });

            int total = scores.Count;
            int pass = scores.Count(s => s == "Passed");
            int fail = total - pass;

            return Ok(new
            {
                subjectId,
                total,
                pass,
                fail,
                passRate = pass * 100.0 / total,
                failRate = fail * 100.0 / total
            });
        }

        // ============================================================
        // 4. DANH SÁCH SINH VIÊN CẢNH BÁO HỌC VỤ (GPA THẤP)
        // ============================================================
        [HttpGet("warning")]
        [Authorize(Roles = "Admin,Teacher")]   // 👈 Sinh viên không được xem danh sách cảnh báo
        public async Task<IActionResult> GetWarningStudents(double gpaThreshold = 2.0)
        {
            var data = await _context.Gpas
                .Include(g => g.Student)
                    .ThenInclude(s => s.User)
                .GroupBy(g => new
                {
                    g.Student.StudentId,
                    g.Student.User.Name
                })
                .Select(g => new
                {
                    StudentId = g.Key.StudentId.Trim(),
                    Name = g.Key.Name.Trim(),
                    AvgGPA = g.Average(x => x.Gpa1)
                })
                .Where(s => s.AvgGPA <= gpaThreshold)
                .ToListAsync();

            return Ok(data);
        }

        // ============================================================
        // 5. GPA TRUNG BÌNH CỦA MỘT KỲ
        // ============================================================
        [HttpGet("gpa/by-semester/{semesterId}")]
        public async Task<IActionResult> GetGpaBySemester(string semesterId)
        {
            var gpas = await _context.Gpas
                .Where(g => g.Semesterid.Trim() == semesterId.Trim())
                .Select(g => g.Gpa1)
                .ToListAsync();

            if (gpas.Count == 0)
                return Ok(new { semesterId, avgGPA = 0 });

            return Ok(new
            {
                semesterId,
                avgGPA = gpas.Average()
            });
        }

        // ============================================================
        // 7. THỐNG KÊ GIẢNG VIÊN
        // ============================================================
        [HttpGet("teacher/{teacherId}")]
        [Authorize(Roles = "Admin,Teacher")] 
        public async Task<IActionResult> GetTeacherStatistics(string teacherId)
        {
            // Lớp đang dạy
            var classes = await _context.Classes
                .Where(c => c.TeacherId.Trim() == teacherId.Trim() && c.Status == "Open")
                .ToListAsync();

            int totalClasses = classes.Count;
            int totalStudents = (int)classes.Sum(c => c.CurrentStudents);

            var scores = await _context.StudentSubjects
                .Include(ss => ss.Class)
                .Where(ss => ss.Class.TeacherId.Trim() == teacherId.Trim())
                .Select(ss => ss.PointTotal)
                .ToListAsync();

            double passRate = 0, failRate = 0;

            if (scores.Count > 0)
            {
                passRate = scores.Count(s => s >= 4) * 100.0 / scores.Count;
                failRate = 100 - passRate;
            }

            return Ok(new
            {
                teacherId,
                totalClasses,
                totalStudents,
                passRate,
                failRate
            });
        }
        [HttpGet("GetTop3Teachers")]
        public async Task<IActionResult> GetTop3Teachers()
        {
            var result = await _context.StudentSubjects
                .Include(ss => ss.Class)
                .GroupBy(ss => ss.Class.TeacherId)
                .Select(g => new
                {
                    TeacherId = g.Key,
                    TotalClasses = g.Select(x => x.ClassId).Distinct().Count(),
                    TotalStudents = g.Count(),
                    PassRate = g.Count(x => x.PointTotal >= 4) * 100.0 / g.Count()
                })
                .OrderByDescending(x => x.PassRate)
                .Take(3)
                .ToListAsync();

            return Ok(result);
        }

    }
}
