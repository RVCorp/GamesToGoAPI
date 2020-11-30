using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GamesToGo.API.Models;

namespace GamesToGo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ReportsController : UserAwareController
    {
        public ReportsController(GamesToGoContext context) : base(context)
        {
        }

        // GET: api/Reports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReport()
        {
            return await Context.Report.ToListAsync();
        }

        // GET: api/Reports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(int id)
        {
            var report = await Context.Report.FindAsync(id);

            if (report == null)
            {
                return NotFound();
            }

            return report;
        }

        // PUT: api/Reports/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReport(int id, Report report)
        {
            if (id != report.Id)
            {
                return BadRequest();
            }

            Context.Entry(report).State = EntityState.Modified;

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(id))
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

        // POST: api/Reports
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("ReportGame")]
        public async Task<IActionResult> ReportGame([FromForm] string reason, [FromForm] string gameID, [FromForm] string type)
        {
            if (!int.TryParse(type, out int typeID))
                return BadRequest();
            if (!int.TryParse(gameID, out int gameIDUseful))
                return BadRequest(); 
            var typeObject = await Context.ReportType.FindAsync(typeID);
            if (typeObject == null)
                return BadRequest("No such report type");
            var gameObject = await Context.Game.FindAsync(gameIDUseful);
            if (gameObject == null)
                return NotFound("No such gameID");
            await Context.Report.AddAsync(new Report
            {
                ReportType = typeObject,
                Reason = string.IsNullOrEmpty(reason) ? String.Empty : reason,
                Game = gameObject,
                User = await Context.User.FindAsync(LoggedUser.Id),
            });
            await Context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Reports/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Report>> DeleteReport(int id)
        {
            var report = await Context.Report.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            Context.Report.Remove(report);
            await Context.SaveChangesAsync();

            return report;
        }

        private bool ReportExists(int id)
        {
            return Context.Report.Any(e => e.Id == id);
        }

        [HttpGet("Available")]
        public async Task<ActionResult<List<ReportType>>> GetAvailableTypes()
        {
            return await Context.ReportType.ToListAsync();
        }
    }
}
