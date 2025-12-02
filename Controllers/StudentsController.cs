using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;  // ⭐ THÊM
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;

namespace QLSV_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]    // ⭐ yêu cầu JWT cho toàn controller
    public class StudentsController : ControllerBase
    {
        private readonly QlsvContext _context;

        public StudentsController(QlsvContext context)
        {
            _context = context;
        }

        // -------------------------
        // LIST BASIC
        // -------------------------
        [HttpGet("list-basic")]
        [AllowAnonymous] // ⭐ cho phép không cần token
        public async Task<IActionResult> GetStudentsBasic()
        {
            var data = await _context.Students
                .Where(s => s.Status == "Active")
                .Include(s => s.User)
                .Include(s => s.Advisor).ThenInclude(a => a.User)
                .Select(s => new {
                    StudentId = s.StudentId.Trim(),
                    Name = s.User != null ? s.User.Name.Trim() : null,
                    Email = s.User != null ? s.User.Email.Trim() : null,
                    AdvisorName = s.Advisor != null ? s.Advisor.User.Name.Trim() : null,
                    Status = s.Status
                })
                .ToListAsync();

            return Ok(data);
        }

        // -------------------------
        // DETAIL
        // -------------------------
        [HttpGet("{id}/detail")]
        [AllowAnonymous]  // ⭐ cho phép public
        public async Task<IActionResult> GetStudentDetail(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest("Id required.");

            var student = await _context.Students
                .Where(s => s.StudentId.Trim() == id.Trim() && s.Status == "Active")
                .Include(s => s.User)
                .Include(s => s.Advisor).ThenInclude(a => a.User)
                .Include(s => s.Gpas)
                .Include(s => s.Conducts)
                .Select(s => new {
                    StudentId = s.StudentId.Trim(),
                    Name = s.User != null ? s.User.Name.Trim() : null,
                    Email = s.User != null ? s.User.Email.Trim() : null,
                    AdvisorName = s.Advisor != null ? s.Advisor.User.Name.Trim() : null,
                    Status = s.Status,
                    GpaAverage = s.Gpas.Any()
                        ? Math.Round(s.Gpas.Average(g => (double?)g.Gpa1) ?? 0, 2)
                        : (double?)null,
                    RecentGPA = s.Gpas.OrderByDescending(g => g.Semesterid)
                                      .Select(g => g.Gpa1)
                                      .FirstOrDefault(),
                    RecentConduct = s.Conducts.OrderByDescending(c => c.SemesterId)
                                              .Select(c => c.Point)
                                              .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (student == null)
                return NotFound();

            return Ok(student);
        }

        // -------------------------
        // EXPORT
        // -------------------------
        [HttpGet("export")]
        [Authorize(Roles = "Admin")] // ⭐ Admin-only
        public async Task<IActionResult> ExportStudents()
        {
            var students = await _context.Students
                .Include(s => s.User).ThenInclude(u => u.Add)
                .Include(s => s.Advisor).ThenInclude(a => a.User)
                .Where(s => s.Status == "Active")
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Students");

            ws.Cell(1, 1).Value = "StudentId";
            ws.Cell(1, 2).Value = "Name";
            ws.Cell(1, 3).Value = "Email";
            ws.Cell(1, 4).Value = "Phone";
            ws.Cell(1, 5).Value = "Birthday";
            ws.Cell(1, 6).Value = "Province";
            ws.Cell(1, 7).Value = "Advisor";
            ws.Cell(1, 8).Value = "Status";

            int row = 2;
            foreach (var s in students)
            {
                ws.Cell(row, 1).Value = s.StudentId?.Trim();
                ws.Cell(row, 2).Value = s.User?.Name?.Trim();
                ws.Cell(row, 3).Value = s.User?.Email?.Trim();
                ws.Cell(row, 4).Value = s.User?.PhoneNumber;
                ws.Cell(row, 5).Value = s.User?.Birthday?.ToString("yyyy-MM-dd");
                ws.Cell(row, 6).Value = s.User?.Add?.Province?.Trim();
                ws.Cell(row, 7).Value = s.Advisor?.User?.Name?.Trim();
                ws.Cell(row, 8).Value = s.Status;
                row++;
            }

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var content = stream.ToArray();

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "students.xlsx");
        }

        // -------------------------
        // UPDATE Student
        // -------------------------
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")] // ⭐ Admin + Teacher
        public async Task<IActionResult> PutStudent(string id, StudentUpdateDto dto)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId.Trim() == id.Trim());

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

        // -------------------------
        // CREATE STUDENT
        // -------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]  // ⭐ Chỉ Admin
        public async Task<IActionResult> PostStudent(StudentCreateDto dto)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                return BadRequest(new { message = $"UserId {dto.UserId} không tồn tại." });

            var advisorExists = !string.IsNullOrWhiteSpace(dto.AdvisorId)
                ? await _context.Advisors.AnyAsync(a => a.AdvisorId.Trim() == dto.AdvisorId.Trim())
                : true;
            if (!advisorExists)
                return BadRequest(new { message = $"AdvisorId {dto.AdvisorId} không tồn tại." });

            var studentExists = await _context.Students.AnyAsync(s => s.UserId == dto.UserId);
            if (studentExists)
                return BadRequest(new { message = $"UserId {dto.UserId} đã được gán Student." });

            string newStudentId = (await GenerateStudentId()).PadRight(30);

            var student = new Student
            {
                StudentId = newStudentId,
                UserId = dto.UserId,
                AdvisorId = dto.AdvisorId,
                Status = "Active"
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var response = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Advisor).ThenInclude(a => a.User)
                .Where(s => s.StudentId.Trim() == newStudentId.Trim())
                .Select(s => new {
                    StudentId = s.StudentId.Trim(),
                    Name = s.User.Name,
                    Email = s.User.Email,
                    AdvisorName = s.Advisor != null ? s.Advisor.User.Name : null,
                    Status = s.Status
                })
                .FirstOrDefaultAsync();

            return Ok(response);
        }

        // -------------------------
        // CREATE FULL
        // -------------------------
        [HttpPost("create-full")]
        [Authorize(Roles = "Admin")]  // ⭐ Admin only
        public async Task<IActionResult> CreateStudentFull([FromBody] StudentCreateFullDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Name and Email are required.");

            if (!string.IsNullOrWhiteSpace(dto.AdvisorId))
            {
                if (!await _context.Advisors.AnyAsync(a => a.AdvisorId.Trim() == dto.AdvisorId.Trim()))
                    return BadRequest($"AdvisorId {dto.AdvisorId} không tồn tại.");
            }

            string accId = null;
            string userId = "usr-" + Guid.NewGuid().ToString("N")[..6];

            if (dto.CreateAccount)
            {
                string username = dto.Username;
                if (string.IsNullOrWhiteSpace(username)) username = userId;

                if (await _context.Accounts.AnyAsync(a => a.Username == username))
                    return BadRequest($"Username {username} đã tồn tại.");

                var account = new Account
                {
                    AccId = "Acc-" + Guid.NewGuid().ToString("N")[..8],
                    Username = username,
                    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password ?? "ChangeMe123"),
                    Role = "Student",
                    Status = "Active",
                    DateCreate = DateOnly.FromDateTime(DateTime.Now),
                    CreateBy = "Admin"
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
                accId = account.AccId;
            }

            var user = new User
            {
                Id = userId,
                Name = dto.Name,
                Email = dto.Email,
                PhoneNumber = int.Parse(dto.PhoneNumber),
                Birthday = dto.Birthday,
                AccId = accId
            };
            _context.Users.Add(user);

            string studentId = (await GenerateStudentId()).PadRight(30);

            var student = new Student
            {
                StudentId = studentId,
                UserId = userId,
                AdvisorId = dto.AdvisorId,
                Status = "Active"
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                StudentId = student.StudentId.Trim(),
                Username = dto.CreateAccount ? (dto.Username ?? userId) : null,
                DefaultPassword = dto.CreateAccount ? "Use provided/ChangeMe123" : null
            });
        }

        // -------------------------
        // UPDATE Student INFO
        // -------------------------
        [HttpPut("{id}/info")]
        [Authorize(Roles = "Admin,Teacher")]  // ⭐ Admin + Teacher
        public async Task<IActionResult> UpdateStudentInfo(string id, [FromBody] StudentUpdateInfoDto dto)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId.Trim() == id.Trim());

            if (student == null) return NotFound();
            if (student.Status == "Inactive") return BadRequest("Student is deleted.");

            if (student.User != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Name)) student.User.Name = dto.Name;
                if (!string.IsNullOrWhiteSpace(dto.Email)) student.User.Email = dto.Email;
                if (dto.PhoneNumber != null) student.User.PhoneNumber = dto.PhoneNumber;
                if (dto.Birthday != null) student.User.Birthday = dto.Birthday;
            }

            if (!string.IsNullOrWhiteSpace(dto.AdvisorId))
            {
                if (!await _context.Advisors.AnyAsync(a => a.AdvisorId.Trim() == dto.AdvisorId.Trim()))
                    return BadRequest($"AdvisorId {dto.AdvisorId} không tồn tại.");
                student.AdvisorId = dto.AdvisorId;
            }

            if (!string.IsNullOrWhiteSpace(dto.Status)) student.Status = dto.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // -------------------------
        // SOFT DELETE
        // -------------------------
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]  // ⭐ Admin Only
        public async Task<IActionResult> SoftDeleteStudent(string id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId.Trim() == id.Trim());
            if (student == null) return NotFound();

            student.Status = "Inactive";
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // -------------------------
        // IMPORT
        // -------------------------
        [HttpPost("import")]
        [Authorize(Roles = "Admin")] // ⭐ Admin Only
        public async Task<IActionResult> ImportStudents(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ.");

            var results = new List<ImportResult>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheet(1);

            int row = 2;
            while (!ws.Cell(row, 1).IsEmpty())
            {
                try
                {
                    string excelStudentId = ws.Cell(row, 1).GetString().Trim();
                    string name = ws.Cell(row, 2).GetString().Trim();
                    string email = ws.Cell(row, 3).GetString().Trim();
                    string phoneStr = ws.Cell(row, 4).GetString().Trim();
                    var bCell = ws.Cell(row, 5);
                    string advisorId = ws.Cell(row, 6).GetString().Trim();
                    string username = ws.Cell(row, 7).GetString().Trim();
                    string password = ws.Cell(row, 8).GetString().Trim();
                    string status = ws.Cell(row, 9).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(name)) name = null;
                    if (string.IsNullOrWhiteSpace(email)) email = null;
                    if (string.IsNullOrWhiteSpace(status)) status = "Active";

                    int? phone = null;
                    if (int.TryParse(phoneStr, out var p)) phone = p;

                    DateOnly? birthday = null;
                    if (bCell.DataType == XLDataType.DateTime)
                        birthday = DateOnly.FromDateTime(bCell.GetDateTime());
                    else if (DateTime.TryParse(bCell.GetString(), out var bd))
                        birthday = DateOnly.FromDateTime(bd);

                    if (!string.IsNullOrWhiteSpace(advisorId) &&
                        !await _context.Advisors.AnyAsync(a => a.AdvisorId.Trim() == advisorId.Trim()))
                    {
                        advisorId = null;
                    }

                    if (string.IsNullOrWhiteSpace(username) ||
                        await _context.Accounts.AnyAsync(a => a.Username == username))
                        username = null;

                    if (string.IsNullOrWhiteSpace(password))
                        password = "ChangeMe123";

                    string accId = null;
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        var account = new Account
                        {
                            AccId = "Acc-" + Guid.NewGuid().ToString("N")[..8],
                            Username = username,
                            Password = BCrypt.Net.BCrypt.HashPassword(password),
                            Role = "Student",
                            Status = "Active"
                        };
                        _context.Accounts.Add(account);
                        await _context.SaveChangesAsync();
                        accId = account.AccId;
                    }

                    string newUserId = "usr-" + Guid.NewGuid().ToString("N")[..6];
                    var user = new User
                    {
                        Id = newUserId,
                        Name = name,
                        Email = email,
                        PhoneNumber = phone,
                        Birthday = birthday,
                        AccId = accId
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    string finalStudentId;
                    if (!string.IsNullOrWhiteSpace(excelStudentId))
                    {
                        if (await _context.Students.AnyAsync(s => s.StudentId.Trim() == excelStudentId.Trim()))
                        {
                            results.Add(new ImportResult { Row = row, Success = false, Message = $"StudentId {excelStudentId} bị trùng." });
                            row++;
                            continue;
                        }
                        finalStudentId = excelStudentId;
                    }
                    else
                    {
                        finalStudentId = await GenerateStudentId();
                    }

                    finalStudentId = finalStudentId.PadRight(30);

                    var student = new Student
                    {
                        StudentId = finalStudentId,
                        UserId = newUserId,
                        AdvisorId = string.IsNullOrWhiteSpace(advisorId) ? null : advisorId,
                        Status = status
                    };

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();

                    results.Add(new ImportResult { Row = row, Success = true, Message = "OK" });
                }
                catch (Exception ex)
                {
                    results.Add(new ImportResult { Row = row, Success = false, Message = ex.Message });
                }

                row++;
            }

            return Ok(results);
        }

        // -------------------------
        // SEARCH / FILTER
        // -------------------------
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Teacher")]  // ⭐ Admin + Teacher
        public async Task<IActionResult> SearchStudents(
            [FromQuery] string? name,
            [FromQuery] string? advisorId,
            [FromQuery] string? subjectId,
            [FromQuery] string? semesterId,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var q = _context.Students
                .Include(s => s.User)
                .Include(s => s.StudentSubjects).ThenInclude(ss => ss.Class)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(s => s.Status == status);

            if (!string.IsNullOrWhiteSpace(advisorId))
                q = q.Where(s => s.AdvisorId.Trim() == advisorId.Trim());

            if (!string.IsNullOrWhiteSpace(name))
                q = q.Where(s => s.User != null && EF.Functions.Like(s.User.Name, $"%{name}%"));

            if (!string.IsNullOrWhiteSpace(subjectId))
                q = q.Where(s => s.StudentSubjects.Any(ss => ss.SubjectId.Trim() == subjectId.Trim()));

            if (!string.IsNullOrWhiteSpace(semesterId))
                q = q.Where(s => s.StudentSubjects.Any(ss => ss.Class != null && ss.Class.SemesterId.Trim() == semesterId.Trim()));

            var total = await q.CountAsync();

            var items = await q
                .OrderBy(s => s.StudentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    StudentId = s.StudentId.Trim(),
                    Name = s.User != null ? s.User.Name.Trim() : null,
                    Email = s.User != null ? s.User.Email.Trim() : null,
                    AdvisorId = s.AdvisorId,
                    Status = s.Status
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // -------------------------
        // Get by academic status
        // -------------------------
        [HttpGet("by-status/{status}")]
        [Authorize(Roles = "Admin,Teacher")] // ⭐ Admin + Teacher
        public async Task<IActionResult> GetByAcademicStatus(string status)
        {
            var items = await _context.Students
                .Where(s => s.AcademicStatus == status)
                .Include(s => s.User)
                .Select(s => new
                {
                    StudentId = s.StudentId.Trim(),
                    Name = s.User != null ? s.User.Name.Trim() : null,
                    Email = s.User != null ? s.User.Email.Trim() : null,
                    AcademicStatus = s.AcademicStatus
                }).ToListAsync();

            return Ok(items);
        }

        // -------------------------
        // Update AcademicStatus batch
        // -------------------------
        [HttpPut("update-academic-status")]
        [Authorize(Roles = "Admin")] // ⭐ Admin Only
        public async Task<IActionResult> UpdateAcademicStatus([FromQuery] string subjectId = "IT6129")
        {
            var passedStudentIds = await _context.StudentSubjects
                .Where(ss => ss.SubjectId.Trim() == subjectId.Trim() && ss.Status.Trim() == "Passed")
                .Select(ss => ss.StudentId.Trim())
                .Distinct()
                .ToListAsync();

            var students = await _context.Students.ToListAsync();

            foreach (var s in students)
            {
                s.AcademicStatus = passedStudentIds.Contains(s.StudentId.Trim()) ? "Graduated" : "Studying";
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật AcademicStatus hoàn tất.", updated = students.Count });
        }

        // -------------------------
        // Restore student
        // -------------------------
        [HttpPut("restore/{id}")]
        [Authorize(Roles = "Admin")] // ⭐ Admin Only
        public async Task<IActionResult> RestoreStudent(string id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.Status = "Active";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // -------------------------
        // HELPERS
        // -------------------------
        private async Task<string> GenerateStudentId()
        {
            var ids = await _context.Students
                .Select(s => s.StudentId.Trim())
                .ToListAsync();

            int max = 0;

            foreach (var id in ids)
            {
                if (id.StartsWith("Stu-") && int.TryParse(id.Substring(4), out int num))
                {
                    if (num > max)
                        max = num;
                }
            }

            int next = max + 1;
            return $"Stu-{next:D5}";
        }

        private bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.StudentId.Trim() == id.Trim());
        }
    }
}
