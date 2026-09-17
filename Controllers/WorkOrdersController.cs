using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MesLite.Data;
using MesLite.Models;
using MesLite.DTOs;

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

        // 工單詳情：BOM 展開需求 + 已領料明細 + 已報工紀錄，一次給前端
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var wo = await _db.WorkOrders.Include(w => w.Product).FirstOrDefaultAsync(w => w.Id == id);
            if (wo == null) return NotFound("工單不存在");

            // BOM 展開：這張工單的產品，生產 TargetQty 需要哪些料、要多少、現有庫存多少
            var bomRequirement = await _db.Boms
                .Where(b => b.ParentItemId == wo.ProductId)
                .Include(b => b.ChildItem)
                .Select(b => new
                {
                    materialId = b.ChildItemId,
                    itemCode = b.ChildItem!.ItemCode,
                    name = b.ChildItem!.Name,
                    requiredPerUnit = b.RequiredQty,
                    requiredTotal = b.RequiredQty * wo.TargetQty,
                    currentStock = b.ChildItem!.StockQty
                }).ToListAsync();

            // 已領料明細（type = ISSUE）
            var issuedTransactions = await _db.InventoryTransactions
                .Where(t => t.WoId == id && t.Type == "ISSUE")
                .Include(t => t.Material)
                .Select(t => new
                {
                    t.Id,
                    t.MaterialId,
                    materialName = t.Material!.Name,
                    t.Qty,
                    t.OperatorName,
                    t.CreatedAt
                }).ToListAsync();

            // 已報工紀錄
            var workReports = await _db.WorkReports
                .Where(r => r.WoId == id)
                .OrderByDescending(r => r.Id)
                .Select(r => new
                {
                    r.Id,
                    r.OperatorName,
                    r.CompletedQty,
                    r.ReportTime
                }).ToListAsync();

            var totalReported = workReports.Sum(r => r.CompletedQty);

            return Ok(new
            {
                workOrder = new
                {
                    wo.Id,
                    wo.WoNumber,
                    wo.ProductId,
                    productName = wo.Product!.Name,
                    wo.TargetQty,
                    wo.Status,
                    wo.CreatedAt
                },
                bomRequirement,
                issuedTransactions,
                workReports,
                totalReported,
                remainingQty = wo.TargetQty - totalReported
            });
        }

        [HttpPost]
        public async Task<ActionResult<WorkOrder>> Create(WorkOrderCreateDto dto)
        {
            var wo = new WorkOrder
            {
                WoNumber = dto.WoNumber,
                ProductId = dto.ProductId,
                TargetQty = dto.TargetQty,
                Status = "DRAFT",
                CreatedAt = DateTime.Now
            };
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