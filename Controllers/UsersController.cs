using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;

namespace QLSV_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly QlsvContext _context;

        public UsersController(QlsvContext context)
        {
            _context = context;
        }

        // LẤY DANH SÁCH USER
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            return await _context.Users
                .Where(u => u.Status == "Active")
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Gender,
                    u.Birthday,
                    u.PhoneNumber,
                    u.AccId
                })
                .ToListAsync();
        }

        // LẤY USER THEO ID
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(string id)
        {
            var user = await _context.Users
                .Include(u => u.Add)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();
            if (user.Status == "Inactive") return BadRequest("User is deleted.");

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Gender,
                user.Birthday,
                user.PhoneNumber,
                Address = user.Add == null ? null : new
                {
                    user.Add.Province,
                    user.Add.District,
                    user.Add.Infor
                }
            });
        }

        // UPDATE USER BẰNG ID
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, UserUpdateDto dto)
        {
            var user = await _context.Users.Include(u => u.Add).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            if (user.Status == "Inactive") return BadRequest("User is deleted.");

            UpdateUserData(user, dto);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ==========================  LOGIN USER ONLY  ==========================

        // GET: api/users/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyUserInfo()
        {
            var accId = GetAccIdFromToken();
            if (accId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Add)
                .Where(u => u.AccId == accId)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Gender,
                    u.Birthday,
                    u.PhoneNumber,
                    Address = u.Add == null ? null : new
                    {
                        u.Add.Province,
                        u.Add.District,
                        u.Add.Infor
                    }
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound("Không tìm thấy user.");

            return Ok(user);
        }

        // PUT: api/users/me → cập nhật user theo token
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyUser(UserUpdateDto dto)
        {
            var accId = GetAccIdFromToken();
            if (accId == null) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.Add)
                .FirstOrDefaultAsync(u => u.AccId == accId);

            if (user == null) return NotFound();

            UpdateUserData(user, dto);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công!" });
        }

        private static void UpdateUserData(User user, UserUpdateDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.Gender))
                user.Gender = dto.Gender;

            if (dto.Birthday != null)
                user.Birthday = dto.Birthday;

            if ((dto.PhoneNumber.HasValue))
                user.PhoneNumber = dto.PhoneNumber;

            // Update Address
            if (dto.Address != null && user.Add != null)
            {
                if (!string.IsNullOrEmpty(dto.Address.Province))
                    user.Add.Province = dto.Address.Province;

                if (!string.IsNullOrEmpty(dto.Address.District))
                    user.Add.District = dto.Address.District;

                if (!string.IsNullOrEmpty(dto.Address.Infor))
                    user.Add.Infor = dto.Address.Infor;
            }
        }
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(UserCreateDto dto)
        {
            // 1. Kiểm tra account tồn tại
            var accExists = await _context.Accounts.AnyAsync(a => a.AccId == dto.AccId);
            if (!accExists)
                return BadRequest($"Không tồn tại account id {dto.AccId}");

            // 2. Generate UserId random không trùng
            string newUserId;
            do
            {
                string randomPart = Guid.NewGuid().ToString("N").Substring(0, 5);
                newUserId = $"usr-{randomPart}";
            }
            while (await _context.Users.AnyAsync(u => u.Id == newUserId));

            // 3. Tạo Address
            var newAddress = new Address
            {
                AddId = "Add-" + Guid.NewGuid().ToString("N").Substring(0, 6),
                Province = dto.Address.Province,
                District = dto.Address.District,
                Infor = dto.Address.Infor
            };
            _context.Addresses.Add(newAddress);

            // 4. Tạo User
            var user = new User
            {
                Id = newUserId,
                Name = dto.Name,
                Email = dto.Email,
                Gender = dto.Gender,
                Birthday = dto.Birthday,
                PhoneNumber = dto.PhoneNumber,
                AccId = dto.AccId,
                AddId = newAddress.AddId,
                Status = "Active"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 5. Trả về
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Gender,
                user.PhoneNumber,
                user.AccId,
                Address = new
                {
                    newAddress.AddId,
                    newAddress.Province,
                    newAddress.District,
                    newAddress.Infor
                }
            });
        }


        private string GetAccIdFromToken()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
