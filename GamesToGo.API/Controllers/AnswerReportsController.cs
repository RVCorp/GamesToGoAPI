using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamesToGo.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AnswerReportsController : UserAwareController
    {
        public AnswerReportsController(GamesToGoContext context) : base(context)
        {
        }
        
        // GET: api/AnswerReports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnswerReport>>> GetAnswerReport()
        {
            return await Context.AnswerReport.ToListAsync();
        }

        // GET: api/AnswerReports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AnswerReport>> GetAnswerReport(int id)
        {
            var answerReport = await Context.AnswerReport.FindAsync(id);

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

            Context.Entry(answerReport).State = EntityState.Modified;

            try
            {
                await Context.SaveChangesAsync();
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
            Context.AnswerReport.Add(answerReport);
            await Context.SaveChangesAsync();

            return CreatedAtAction("GetAnswerReport", new { id = answerReport.Id }, answerReport);
        }

        // DELETE: api/AnswerReports/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<AnswerReport>> DeleteAnswerReport(int id)
        {
            var answerReport = await Context.AnswerReport.FindAsync(id);
            if (answerReport == null)
            {
                return NotFound();
            }

            Context.AnswerReport.Remove(answerReport);
            await Context.SaveChangesAsync();

            return answerReport;
        }

        private bool AnswerReportExists(int id)
        {
            return Context.AnswerReport.Any(e => e.Id == id);
        }
    }
}
