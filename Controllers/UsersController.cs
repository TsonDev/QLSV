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
            return await _context.Users.ToListAsync();
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

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /* [HttpPost]
         public async Task<ActionResult<User>> PostUser(User user)
         {
             _context.Users.Add(user);
             try
             {
                 await _context.SaveChangesAsync();
             }
             catch (DbUpdateException)
             {
                 if (UserExists(user.Id))
                 {
                     return Conflict();
                 }
                 else
                 {
                     throw;
                 }
             }

             return CreatedAtAction("GetUser", new { id = user.Id }, user);
         }*/
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] UserCreateDto dto)
        {
            var accExists = await _context.Accounts.AnyAsync(a => a.AccId == dto.AccId);
            if (!accExists)
            {
                return BadRequest($"Không tồn tại account id{dto.AccId}");
            }
            var newAddress = new Address
            {
                AddId = "Add-" + Guid.NewGuid().ToString("N").Substring(0, 6),
                Province = dto.Address.Province,
                District = dto.Address.District,
                Infor = dto.Address.Infor
            };
            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();
            var user = new User{
               Id = dto.Id,
               Name = dto.Name,
               Email = dto.Email,
               Birthday = dto.Birthday,
               Gender = dto.Gender,
               PhoneNumber = dto.PhoneNumber,
               AccId = dto.AccId,
               AddId = newAddress.AddId,
              
           };
           _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (UserExists(user.Id))
                {
                    return Conflict(new {message = $"UserID{user.Id} đã tồn tại"});
                }
                throw;
            }
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

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
