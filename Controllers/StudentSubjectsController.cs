using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;
using System.ComponentModel.DataAnnotations.Schema;


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
                    IsApproved = !_context.StudentSubjects.Any(ss => ss.ClassId == c.Id && ss.IsApproved != 1),
                    ApprovedAt = _context.StudentSubjects.Where(ss => ss.ClassId == c.Id).Max(ss => ss.ApprovedAt),
                    ApprovedBy = _context.StudentSubjects.Where(ss => ss.ClassId == c.Id).Max(ss => ss.ApprovedBy)
                });

            // Nếu FE truyền approved=true/false → lọc
            if (approved.HasValue)
                query = query.Where(c => c.IsApproved == approved.Value);

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

        [HttpPut("class/{classId}/approve")]
        public async Task<IActionResult> ApproveClass(int classId, [FromQuery] string accId)
        {
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



    }
}
