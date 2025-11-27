using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;

namespace QLSV_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly QlsvContext _context;

        public AccountsController(QlsvContext context)
        {
            _context = context;
        }

        // ============================
        // GET ALL ACTIVE ACCOUNTS
        // ============================
        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var data = await _context.Accounts
                .Where(a => a.Status == "Active")
                .Select(a => new {
                    a.AccId,
                    a.Username,
                    a.Role,
                    a.Status,
                    a.DateCreate
                })
                .ToListAsync();

            return Ok(data);
        }

        // ============================
        // GET DETAIL
        // ============================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(string id)
        {
            var account = await _context.Accounts
                .Where(a => a.AccId == id && a.Status == "Active")
                .FirstOrDefaultAsync();

            if (account == null)
                return NotFound();

            return Ok(account);
        }

        // ============================
        // CREATE ACCOUNT
        // ============================
        [HttpPost]
        public async Task<IActionResult> PostAccount(AccountCreateDto dto)
        {
            if (await _context.Accounts.AnyAsync(a => a.Username == dto.Username))
                return BadRequest("Username đã tồn tại.");

            // === KHÔNG DÙNG ID TĂNG SỐ NỮA ===
            string newId = $"acc-{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            var acc = new Account
            {
                AccId = newId,
                Username = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role ?? "User",
                Status = "Active",
                DateCreate = DateOnly.FromDateTime(DateTime.Now),
                CreateBy = "Admin"
            };

            _context.Accounts.Add(acc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo tài khoản thành công!", id = newId });
        }



        // ============================
        // UPDATE ACCOUNT (Không sửa password ở đây)
        // ============================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(string id, AccountUpdateDto dto)
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.Username))
                acc.Username = dto.Username;

            if (!string.IsNullOrEmpty(dto.Role))
                acc.Role = dto.Role;

            if (!string.IsNullOrEmpty(dto.Status))
                acc.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok("Cập nhật thành công");
        }

        // ============================
        // RESET PASSWORD
        // ============================
        [HttpPut("reset-password/{id}")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] AccountActionDto dto)
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null) return NotFound();

            acc.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();
            return Ok("Đặt lại mật khẩu thành công!");
        }

        // ============================
        // UPDATE ROLE
        // ============================
        [HttpPut("update-role/{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] AccountActionDto dto)
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null) return NotFound();

            acc.Role = dto.Role;
            await _context.SaveChangesAsync();

            return Ok("Cập nhật quyền thành công!");
        }

        // ============================
        // LOCK ACCOUNT
        // ============================
        [HttpPut("lock/{id}")]
        public async Task<IActionResult> LockAccount(string id)
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null) return NotFound();

            acc.Status = "Inactive";
            await _context.SaveChangesAsync();
            return Ok("Đã khóa tài khoản.");
        }

        // ============================
        // UNLOCK ACCOUNT
        // ============================
        [HttpPut("unlock/{id}")]
        public async Task<IActionResult> UnlockAccount(string id)
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null) return NotFound();

            acc.Status = "Active";
            await _context.SaveChangesAsync();
            return Ok("Đã mở khóa tài khoản.");
        }

        // ============================
        // SOFT DELETE
        // ============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null) return NotFound();

            acc.Status = "Inactive";
            await _context.SaveChangesAsync();

            return Ok("Đã xóa tài khoản.");
        }
    }
}
