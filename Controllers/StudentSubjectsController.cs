using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;


namespace QLSV_V1.Controllers
{
   
    [Route("api/[controller]")]
    [ApiController]
    public class StudentSubjectsController : ControllerBase
    {
        private readonly QlsvContext _context;

        public StudentSubjectsController(QlsvContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("class/status")]
        public async Task<IActionResult> GetClassApprovalStatus([FromQuery] bool? approved)
        {
            var query = _context.Classes
                .Select(c => new
                {
                    ClassId = c.Id,
                    c.ClassName,
                    SubjectId = c.SubjectId,
                    TotalRecords = _context.StudentSubjects.Count(ss => ss.ClassId == c.Id),

                    // TRUE nếu tất cả điểm đều IsApproved = 1
                    IsApproved = !_context.StudentSubjects.Any(ss => ss.ClassId == c.Id && ss.IsApproved != 1),

                    // Kiểm tra GV đã gửi chưa
                    HasSubmitted = _context.StudentSubjects.Any(ss =>
                        ss.ClassId == c.Id && ss.Status == "submitted"),

                    ApprovedAt = _context.StudentSubjects
                        .Where(ss => ss.ClassId == c.Id)
                        .Max(ss => ss.ApprovedAt),

                    ApprovedBy = _context.StudentSubjects
                        .Where(ss => ss.ClassId == c.Id)
                        .Max(ss => ss.ApprovedBy)
                });

            // Lọc theo trạng thái gửi từ FE
            if (approved.HasValue)
            {
                if (approved.Value)
                {
                    // ===========================
                    // Lọc: ĐÃ DUYỆT
                    // ===========================
                    query = query.Where(c => c.IsApproved);
                }
                else
                {
                    // ===========================
                    // Lọc: CHƯA DUYỆT nhưng đã gửi
                    // ===========================
                    query = query.Where(c => !c.IsApproved && c.HasSubmitted);
                }
            }

            var data = await query.ToListAsync();
            return Ok(data);
        }

        [HttpGet("class/status/unapproved")]
        public async Task<IActionResult> GetUnapprovedClasses()
        {
            return await GetClassApprovalStatus(false);
        }

        [HttpGet("class/status/approved")]
        public async Task<IActionResult> GetApprovedClasses()
        {
            return await GetClassApprovalStatus(true);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("class/{classId}/approve")]
        public async Task<IActionResult> ApproveClass(int classId)
        {
            var accId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(accId))
                return BadRequest("Thiếu accId.");

            // 1) Kiểm tra accId có tồn tại không
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccId.Trim() == accId.Trim());

            if (account == null)
                return BadRequest("Tài khoản không tồn tại.");

            // 2) Kiểm tra role = admin chưa?
            //if (!string.Equals(account.Role?.Trim(), "admin", StringComparison.OrdinalIgnoreCase))
            //    return BadRequest("Chỉ admin mới được duyệt điểm.");

            // 3) Lấy thông tin lớp
            var classInfo = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId);

            if (classInfo == null)
                return NotFound("Không tìm thấy lớp.");

            // 4) Lấy danh sách điểm lớp
            var list = await _context.StudentSubjects
                .Where(ss => ss.ClassId == classId)
                .ToListAsync();

            if (!list.Any())
                return BadRequest("Lớp này chưa có dữ liệu điểm.");

            // 5) Ngày duyệt = Ngày tạo lớp + 4 tháng
            if (classInfo.DateCreate == null)
                return BadRequest("Lớp chưa có DateCreate.");
            DateOnly dateCreate = classInfo.DateCreate.Value;
            DateTime approveDate = dateCreate.ToDateTime(TimeOnly.MinValue).AddMonths(4);


            foreach (var ss in list)
            {
                ss.IsApproved = 1;
                ss.ApprovedBy = account.AccId.Trim();
                ss.ApprovedAt = approveDate;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Duyệt điểm thành công",
                classId,
                approvedAt = approveDate,
                approvedBy = accId,
                total = list.Count
            });
        }
        [HttpGet("class/{classId}/export")]
        public async Task<IActionResult> ExportScoresByClassToExcel(int classId)
        {
            var data = await (
                from ss in _context.StudentSubjects
                join s in _context.Students on ss.StudentId equals s.StudentId
                join u in _context.Users on s.UserId equals u.Id into userJoin
                from u in userJoin.DefaultIfEmpty()
                join sub in _context.Subjects on ss.SubjectId equals sub.Id
                where ss.ClassId == classId
                select new
                {
                    ss.StudentId,
                    StudentName = u.Name,
                    SubjectName = sub.Name,
                    ss.Point1,
                    ss.Point2,
                    ss.Point3,
                    ss.PointTotal,
                    ss.IsApproved,
                    ss.ApprovedBy,
                    ss.ApprovedAt
                }
            ).ToListAsync();

            if (!data.Any())
                return BadRequest("Không có dữ liệu để xuất.");

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.AddWorksheet("BangDiem");

            // Header
            ws.Cell("A1").Value = "STT";
            ws.Cell("B1").Value = "Mã SV";
            ws.Cell("C1").Value = "Tên sinh viên";
            ws.Cell("D1").Value = "Môn học";
            ws.Cell("E1").Value = "Điểm 1";
            ws.Cell("F1").Value = "Điểm 2";
            ws.Cell("G1").Value = "Điểm 3";
            ws.Cell("H1").Value = "Tổng điểm";
            ws.Cell("I1").Value = "Trạng thái";
            ws.Cell("J1").Value = "Người duyệt";
            ws.Cell("K1").Value = "Ngày duyệt";

            ws.Range("A1:K1").Style.Font.Bold = true;
            ws.Range("A1:K1").Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.LightGray);

            int row = 2;
            int stt = 1;

            foreach (var ss in data)
            {
                ws.Cell(row, 1).Value = stt++;
                ws.Cell(row, 2).Value = ss.StudentId;
                ws.Cell(row, 3).Value = ss.StudentName;
                ws.Cell(row, 4).Value = ss.SubjectName;
                ws.Cell(row, 5).Value = ss.Point1;
                ws.Cell(row, 6).Value = ss.Point2;
                ws.Cell(row, 7).Value = ss.Point3;
                ws.Cell(row, 8).Value = ss.PointTotal;
                ws.Cell(row, 9).Value = ss.IsApproved == 1 ? "Đã duyệt" : "Chưa duyệt";
                ws.Cell(row, 10).Value = ss.ApprovedBy;
                ws.Cell(row, 11).Value = ss.ApprovedAt?.ToString("dd/MM/yyyy");
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"BangDiem_Lop_{classId}.xlsx"
            );
        }
        [HttpPut("student-subject/{studentId}/{subjectId}/{semesterId}")]
        public async Task<IActionResult> UpdateScore(
     string studentId,
     string subjectId,
     string semesterId,
     [FromBody] ScoreUpdateDto dto)
        {
            var ss = await _context.StudentSubjects
                .FirstOrDefaultAsync(x =>
                    x.StudentId == studentId &&
                    x.SubjectId == subjectId &&
                    x.SemesterId == semesterId);

            if (ss == null)
                return NotFound("Không tìm thấy bản ghi điểm.");

            if (ss.IsApproved == 1)
                return BadRequest("Điểm đã duyệt, giảng viên không thể sửa.");

            // Cập nhật điểm
            ss.Point1 = dto.Point1;
            ss.Point2 = dto.Point2;
            ss.Point3 = dto.Point3;
            ss.SoTietNghi = dto.SoTietNghi;
            ss.SoTiet = dto.SoTiet;

            ss.PointTotal =
                (dto.Point1 ?? 0) * 0.3 +
                (dto.Point2 ?? 0) * 0.3 +
                (dto.Point3 ?? 0) * 0.4;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật điểm thành công",
                ss.StudentId,
                ss.SubjectId,
                ss.SemesterId,
                ss.Point1,
                ss.Point2,
                ss.Point3,
                ss.PointTotal,
                ss.SoTiet,
                ss.SoTietNghi
            });
        }



        [HttpGet("student/{studentId}/export")]
        public async Task<IActionResult> ExportScoresByStudentToExcel(string studentId)
        {
            var data = await _context.StudentSubjects
                .Where(ss => ss.StudentId == studentId)
                .Include(ss => ss.Subject)
                .Include(ss => ss.Semester)
                .Include(ss => ss.Class)
                .ToListAsync();

            if (!data.Any())
                return BadRequest("Không có dữ liệu để xuất.");

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.AddWorksheet("BangDiemSV");

            // Info
            ws.Cell("A1").Value = "Mã sinh viên:";
            ws.Cell("B1").Value = studentId;

            // Header
            ws.Cell("A3").Value = "Môn học";
            ws.Cell("B3").Value = "Học kỳ";
            ws.Cell("C3").Value = "Lớp HP";
            ws.Cell("D3").Value = "Điểm 1";
            ws.Cell("E3").Value = "Điểm 2";
            ws.Cell("F3").Value = "Điểm 3";
            ws.Cell("G3").Value = "Tổng";
            ws.Cell("H3").Value = "Trạng thái";
            ws.Cell("I3").Value = "Người duyệt";
            ws.Cell("J3").Value = "Ngày duyệt";

            ws.Range("A3:J3").Style.Font.Bold = true;
            ws.Range("A3:J3").Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.LightGray);

            int row = 4;

            foreach (var ss in data)
            {
                ws.Cell(row, 1).Value = ss.Subject.Name;
                ws.Cell(row, 2).Value = ss.Semester.Name;
                ws.Cell(row, 3).Value = ss.Class.ClassName;
                ws.Cell(row, 4).Value = ss.Point1;
                ws.Cell(row, 5).Value = ss.Point2;
                ws.Cell(row, 6).Value = ss.Point3;
                ws.Cell(row, 7).Value = ss.PointTotal;
                ws.Cell(row, 8).Value = ss.IsApproved == 1 ? "Đã duyệt" : "Chưa duyệt";
                ws.Cell(row, 9).Value = ss.ApprovedBy;
                ws.Cell(row, 10).Value = ss.ApprovedAt?.ToString("dd/MM/yyyy");

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            string fileName = $"BangDiem_SinhVien_{studentId}.xlsx";

            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        //    [HttpPut("class/{classId}/submit")]
        //    public async Task<IActionResult> SubmitClass(
        //int classId,
        //[FromQuery] string teacherId)   // truyền mã giảng viên gửi điểm
        //    {
        //        if (string.IsNullOrWhiteSpace(teacherId))
        //            return BadRequest("Thiếu teacherId.");

        //        // 1) Kiểm tra tài khoản có tồn tại & đúng vai trò không
        //        var teacherAccount = await _context.Accounts
        //            .FirstOrDefaultAsync(a => a.AccId.Trim() == teacherId.Trim());

        //        if (teacherAccount == null)
        //            return BadRequest("Tài khoản không tồn tại.");

        //        if (!string.Equals(teacherAccount.Role?.Trim(), "teacher", StringComparison.OrdinalIgnoreCase))
        //            return BadRequest("Chỉ giảng viên mới được gửi điểm.");

        //        // 2) Kiểm tra lớp tồn tại
        //        var classInfo = await _context.Classes
        //            .FirstOrDefaultAsync(c => c.Id == classId);

        //        if (classInfo == null)
        //            return NotFound("Không tìm thấy lớp.");

        //        // 3) (Optional) Kiểm tra giảng viên có phụ trách lớp không
        //        //if (classInfo.TeacherId != teacherId)
        //        //    return BadRequest("Bạn không có quyền gửi điểm của lớp này.");

        //        // 4) Lấy danh sách điểm của lớp
        //        var list = await _context.StudentSubjects
        //            .Where(x => x.ClassId == classId)
        //            .ToListAsync();

        //        if (!list.Any())
        //            return BadRequest("Lớp chưa có dữ liệu điểm.");

        //        // 5) Nếu lớp đã duyệt → không cho gửi lại
        //        if (list.Any(x => x.IsApproved == 1))
        //            return BadRequest("Điểm lớp này đã được duyệt, không thể gửi lại.");

        //        // 6) Reset trạng thái để chờ duyệt
        //        foreach (var ss in list)
        //        {
        //            ss.IsApproved = 0;       // Chờ duyệt
        //            ss.ApprovedBy = null;
        //            ss.ApprovedAt = null;
        //            ss.Status = "submitted"; // optional
        //        }

        //        await _context.SaveChangesAsync();

        //        return Ok(new
        //        {
        //            message = "Gửi điểm thành công. Chờ admin duyệt.",
        //            classId,
        //            total = list.Count
        //        });
        //    }

        //[Authorize(Roles = "Teacher")]
        [HttpPut("class/{classId}/submit")]
        public async Task<IActionResult> SubmitClass(int classId)
        {
            var accId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.Trim();
            //string accId = "Acc-00929";

            if (string.IsNullOrWhiteSpace(accId))
                return Unauthorized("Không lấy được accId từ token.");

            // 1) Lấy account và user
            var account = await _context.Accounts
                .Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.AccId.Trim() == accId);

            if (account == null)
                return Unauthorized("Không tìm thấy tài khoản.");

            // 2) Lấy userId của account
            var userIds = account.Users.Select(u => u.Id).ToList();

            // 3) Lấy thông tin giáo viên
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => userIds.Contains(t.UserId));

            if (teacher == null)
                return Unauthorized("Tài khoản này không phải giảng viên.");

            // 4) Lấy thông tin lớp
            var classInfo = await _context.Classes
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classInfo == null)
                return NotFound("Không tìm thấy lớp.");

            // 5) Kiểm tra phụ trách lớp
            if (classInfo.TeacherId.Trim() != teacher.TeacherId.Trim())
                return BadRequest("Bạn không có quyền gửi điểm lớp này.");

            // 6) Lấy danh sách điểm
            var list = await _context.StudentSubjects
                .Where(x => x.ClassId == classId)
                .ToListAsync();

            foreach (var ss in list)
            {
                ss.IsApproved = 0;
                ss.ApprovedBy = null;
                ss.ApprovedAt = null;
                ss.Status = "submitted";
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Gửi điểm thành công", classId });
        }
        [HttpGet("class/{classId}")]
        public async Task<IActionResult> GetScoresByClass(int classId)
        {
            var data = await (
                from ss in _context.StudentSubjects
                join s in _context.Students on ss.StudentId equals s.StudentId
                join u in _context.Users on s.UserId equals u.Id into userJoin
                from u in userJoin.DefaultIfEmpty()
                join sub in _context.Subjects on ss.SubjectId equals sub.Id
                join sem in _context.Semesters on ss.SemesterId equals sem.SemesterId
                where ss.ClassId == classId
                select new
                {
                    ss.StudentId,
                    StudentName = u.Name,
                    ss.SubjectId,
                    SubjectName = sub.Name,
                    ss.SemesterId,
                    SemesterName = sem.Name,
                    ss.Point1,
                    ss.Point2,
                    ss.Point3,
                    ss.PointTotal,
                    ss.SoTietNghi,
                    ss.SoTiet,
                    ss.IsApproved,
                    ss.Status
                }
            ).ToListAsync();

            return Ok(data);
        }







    }
}
