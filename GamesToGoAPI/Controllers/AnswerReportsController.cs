using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GamesToGoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace GamesToGoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AnswerReportsController : ControllerBase
    {
        private readonly GamesToGoContext _context;

        public AnswerReportsController(GamesToGoContext context)
        {
            _context = context;
        }

        // GET: api/AnswerReports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnswerReport>>> GetAnswerReport()
        {
            return await _context.AnswerReport.ToListAsync();
        }

        // GET: api/AnswerReports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AnswerReport>> GetAnswerReport(int id)
        {
            var answerReport = await _context.AnswerReport.FindAsync(id);

            if (answerReport == null)
            {
                return NotFound();
            }

            return answerReport;
        }

        // PUT: api/AnswerReports/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAnswerReport(int id, AnswerReport answerReport)
        {
            if (id != answerReport.Id)
            {
                return BadRequest();
            }

            _context.Entry(answerReport).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnswerReportExists(id))
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

        // POST: api/AnswerReports
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<AnswerReport>> PostAnswerReport(AnswerReport answerReport)
        {
            _context.AnswerReport.Add(answerReport);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAnswerReport", new { id = answerReport.Id }, answerReport);
        }

        // DELETE: api/AnswerReports/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AnswerReport>> DeleteAnswerReport(int id)
        {
            var answerReport = await _context.AnswerReport.FindAsync(id);
            if (answerReport == null)
            {
                return NotFound();
            }

            _context.AnswerReport.Remove(answerReport);
            await _context.SaveChangesAsync();

            return answerReport;
        }

        private bool AnswerReportExists(int id)
        {
            return _context.AnswerReport.Any(e => e.Id == id);
        }
    }
}
