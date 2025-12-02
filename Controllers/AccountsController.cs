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
        // ============================
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var acc = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == dto.Username);

            if (acc == null)
                return BadRequest("Sai tài khoản.");

            
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

            return Ok(new { token = jwt });
        }
    }
}
