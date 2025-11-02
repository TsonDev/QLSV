using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSV_V1.Models;

namespace QLSV_V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvisorsController : ControllerBase
    {
        private readonly QlsvContext _context;

        public AdvisorsController(QlsvContext context)
        {
            _context = context;
        }

        // GET: api/Advisors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Advisor>>> GetAdvisors()
        {
            return await _context.Advisors.ToListAsync();
        }

        // GET: api/Advisors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Advisor>> GetAdvisor(string id)
        {
            var advisor = await _context.Advisors.FindAsync(id);

            if (advisor == null)
            {
                return NotFound();
            }

            return advisor;
        }

        // PUT: api/Advisors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdvisor(string id, Advisor advisor)
        {
            if (id != advisor.AdvisorId)
            {
                return BadRequest();
            }

            _context.Entry(advisor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdvisorExists(id))
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

        // POST: api/Advisors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Advisor>> PostAdvisor(Advisor advisor)
        {
            _context.Advisors.Add(advisor);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (AdvisorExists(advisor.AdvisorId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetAdvisor", new { id = advisor.AdvisorId }, advisor);
        }

        // DELETE: api/Advisors/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdvisor(string id)
        {
            var advisor = await _context.Advisors.FindAsync(id);
            if (advisor == null)
            {
                return NotFound();
            }

            _context.Advisors.Remove(advisor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AdvisorExists(string id)
        {
            return _context.Advisors.Any(e => e.AdvisorId == id);
        }
    }
}
