using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MesLite.Data;
using MesLite.Models;

namespace MesLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkOrdersController : ControllerBase
    {
        private readonly MesLiteContext _db;
        public WorkOrdersController(MesLiteContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] string? status)
        {
            var q = _db.WorkOrders.Include(w => w.Product).AsQueryable();
            if (!string.IsNullOrEmpty(status)) q = q.Where(w => w.Status == status);

            var result = await q.OrderByDescending(w => w.Id).Select(w => new
            {
                w.Id,
                w.WoNumber,
                w.ProductId,
                ProductName = w.Product!.Name,
                w.TargetQty,
                w.Status,
                w.CreatedAt
            }).ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkOrder>> GetById(int id)
        {
            var wo = await _db.WorkOrders.Include(w => w.Product).FirstOrDefaultAsync(w => w.Id == id);
            if (wo == null) return NotFound();
            return wo;
        }

        [HttpPost]
        public async Task<ActionResult<WorkOrder>> Create(WorkOrder wo)
        {
            wo.CreatedAt = DateTime.Now;
            wo.Status = "DRAFT";
            _db.WorkOrders.Add(wo);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = wo.Id }, wo);
        }

        // 狀態機：DRAFT -> ISSUED -> COMPLETED
        // （ISSUED 由 auto-issue 領料成功後自動切換；COMPLETED 由 receipt 成品入庫後自動切換，
        //   這裡開放的手動切換主要給特殊情況/測試用）
        private static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            ["DRAFT"] = new[] { "ISSUED" },
            ["ISSUED"] = new[] { "COMPLETED" },
            ["COMPLETED"] = Array.Empty<string>()
        };

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] string newStatus)
        {
            var wo = await _db.WorkOrders.FindAsync(id);
            if (wo == null) return NotFound();

            if (!AllowedTransitions.TryGetValue(wo.Status, out var allowed) || !allowed.Contains(newStatus))
                return BadRequest($"不允許從 {wo.Status} 切換到 {newStatus}");

            wo.Status = newStatus;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var wo = await _db.WorkOrders.FindAsync(id);
            if (wo == null) return NotFound();
            _db.WorkOrders.Remove(wo);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
