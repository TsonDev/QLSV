using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using QLSV_V1.Models;

namespace QLSV_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly QlsvContext _context;
        private readonly IConfiguration _config;

        public AccountsController(QlsvContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ============================
        // GET ALL ACTIVE ACCOUNTS
        // ============================
        [Authorize(Roles = "Admin")]
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
        // ===========================
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
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> PostAccount(AccountCreateDto dto)
        {
            if (await _context.Accounts.AnyAsync(a => a.Username == dto.Username))
                return BadRequest("Username đã tồn tại.");

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
        // UPDATE ACCOUNT
        // ============================
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(string id, AccountUpdateDto dto)
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.Username))
            {
                bool exists = await _context.Accounts.AnyAsync(a => a.Username == dto.Username && a.AccId != id);
                if (exists) return BadRequest("Username đã tồn tại.");
                acc.Username = dto.Username;
            }


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
        public class ResetPasswordRequest
        {
            public string Username { get; set; }
        }

        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestResetPassword([FromBody] ResetPasswordRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username))
                return BadRequest("Vui lòng nhập username.");

            var acc = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username.Trim() == req.Username.Trim());

            if (acc == null)
                return BadRequest("Tài khoản không tồn tại.");

            // Tạo ticket reset
            var ticket = new ResetTicket
            {
                AccId = acc.AccId,
                Username = acc.Username,
                Status = "Pending",
                RequestedAt = DateTime.Now
            };

            _context.ResetTickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Yêu cầu reset mật khẩu đã được gửi. Admin sẽ xử lý." });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("reset-password/by-ticket/{ticketId}")]
        public async Task<IActionResult> ResetPasswordByTicket(int ticketId)
        {
            var ticket = await _context.ResetTickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound("Không tìm thấy yêu cầu reset.");

            if (ticket.Status != "Pending")
                return BadRequest("Yêu cầu đã được xử lý.");

            var acc = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccId == ticket.AccId);

            if (acc == null)
                return BadRequest("Tài khoản không tồn tại.");

            // Tạo mật khẩu mới
            string newPass = "abc123"; // bạn có thể random hoặc gửi mail

            acc.Password = BCrypt.Net.BCrypt.HashPassword(newPass);

            // Cập nhật ticket
            ticket.Status = "Approved";
            ticket.ProcessedAt = DateTime.Now;
            ticket.ProcessedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Đặt lại mật khẩu thành công!",
                username = acc.Username,
                newPassword = newPass
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("reset-password/reject/{ticketId}")]
        public async Task<IActionResult> RejectReset(int ticketId)
        {
            var ticket = await _context.ResetTickets.FindAsync(ticketId);
            if (ticket == null)
                return NotFound("Không tìm thấy yêu cầu reset.");

            if (ticket.Status != "Pending")
                return BadRequest("Yêu cầu đã được xử lý.");

            ticket.Status = "Rejected";
            ticket.ProcessedAt = DateTime.Now;
            ticket.ProcessedBy = User.Identity.Name;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã từ chối yêu cầu reset." });
        }






        // ============================
        // UPDATE ROLE
        // ============================
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null) return NotFound();

            acc.Status = "Inactive";
            await _context.SaveChangesAsync();

            return Ok("Đã xóa tài khoản.");
        }

        // ============================
        // LOGIN (KHÔNG CẦN JWT)
        // ============================
        [HttpPost("login")]
        [AllowAnonymous]

        public async Task<IActionResult> Login(LoginDto dto)
        {
            //var acc = await _context.Accounts
            //    .FirstOrDefaultAsync(a => a.Username == dto.Username);

            //if (acc == null)
            //    return BadRequest("Sai tài khoản.");
            var acc = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == dto.Username && a.Status == "Active");

            if (acc == null) return BadRequest("Tài khoản bị khóa hoặc không tồn tại.");


            if (!BCrypt.Net.BCrypt.Verify(dto.Password.Trim(), acc.Password.Trim()))
            {
                // Nếu pass DB không phải bcrypt → fallback so sánh plain text
                if (acc.Password.Trim() != dto.Password.Trim())
                    return BadRequest("Sai mật khẩu.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, acc.AccId.Trim()),
                new Claim(ClaimTypes.Role, acc.Role?.Trim() ?? "")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = jwt,
                role = acc.Role?.Trim()
            });
        }
    }
}
