using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
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

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users
                .Where(u=>u.Status=="Active").ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            if (user.Status == "Inactive")
                return BadRequest("User is deleted.");

            return user;
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            if (user.Status == "Inactive")
                return BadRequest("User is deleted.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (dto.Birthday != null)
                user.Birthday = dto.Birthday;

            if (!string.IsNullOrWhiteSpace(dto.Gender))
                user.Gender = dto.Gender;

            if (dto.PhoneNumber != null)
                user.PhoneNumber = dto.PhoneNumber;

            // UPDATE Address
            if (dto.Address != null)
            {
                var address = await _context.Addresses.FindAsync(user.AddId);
                if (address != null)
                {
                    if (!string.IsNullOrWhiteSpace(dto.Address.Province))
                        address.Province = dto.Address.Province;

                    if (!string.IsNullOrWhiteSpace(dto.Address.District))
                        address.District = dto.Address.District;

                    if (!string.IsNullOrWhiteSpace(dto.Address.Infor))
                        address.Infor = dto.Address.Infor;
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(UserCreateDto dto)
        {
            // Kiểm tra account tồn tại
            var accExists = await _context.Accounts.AnyAsync(a => a.AccId == dto.AccId);
            if (!accExists)
                return BadRequest($"Không tồn tại account id {dto.AccId}");

            // Tự sinh AddId
            var newAddress = new Address
            {
                AddId = "Add-" + Guid.NewGuid().ToString("N")[..6],
                Province = dto.Address.Province,
                District = dto.Address.District,
                Infor = dto.Address.Infor
            };

            _context.Addresses.Add(newAddress);

            // Tự sinh UserId
            var lastUser = await _context.Users
                .Where(u => u.Id.StartsWith("usr-"))
                .OrderByDescending(u => u.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastUser != null)
            {
                string numberPart = lastUser.Id.Substring(4);
                nextNumber = int.Parse(numberPart) + 1;
            }

            string newUserId = $"usr-{nextNumber:D5}";

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

            var result = new
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
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, result);
        }


        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Status = "Inactive";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Status = "Active";
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
