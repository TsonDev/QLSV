using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Http;
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

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            return await _context.Accounts
                .Where(a=>a.Status=="Active")
                .ToListAsync();
        }

        // GET: api/Accounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Account>> GetAccount(string id)
        {
            var account = await _context.Accounts
    .Where(a => a.Status == "Active" && a.AccId == id)
    .FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound();
            }

            return account;
        }

        // PUT: api/Accounts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAccount(string id, AccountUpdateDto dto)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            if (account.Status == "Inactive")
                return BadRequest("Account is deleted.");

            if (!string.IsNullOrWhiteSpace(dto.Username))
                account.Username = dto.Username;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                account.Password = dto.Password;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                account.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.Role))
                account.Role = dto.Role;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("restore/{id}")]
        public async Task<IActionResult> RestoreAccount(string id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            account.Status = "Active";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Accounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount(AccountCreateDto dto)
        {
            // Lấy mã lớn nhất
            var lastAcc = await _context.Accounts
                .OrderByDescending(a => a.AccId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastAcc != null)
            {
                string numberPart = lastAcc.AccId.Substring(4); // Bỏ "Acc-"
                nextNumber = int.Parse(numberPart) + 1;
            }

            string newAccId = $"Acc-{nextNumber.ToString("D5")}";

            var acc = new Account
            {
                AccId = newAccId,
                Username = dto.Username,
                Password = dto.Password,
                Role = dto.Role ?? "User",
                Status = "Active",
                DateCreate = DateOnly.FromDateTime(DateTime.Now),
                CreateBy = "Admin"
            };

            _context.Accounts.Add(acc);
            await _context.SaveChangesAsync();

            return Ok(acc);
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            account.Status = "Inactive"; 

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AccountExists(string id)
        {
            return _context.Accounts.Any(e => e.AccId == id);
        }
    }
}
