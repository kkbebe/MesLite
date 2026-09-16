using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MesLite.Data;
using MesLite.Models;
using MesLite.DTOs;

namespace MesLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryTransactionsController : ControllerBase
    {
        private readonly MesLiteContext _db;
        public InventoryTransactionsController(MesLiteContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? woId)
        {
            var q = _db.InventoryTransactions
                .Include(t => t.Material)
                .Include(t => t.WorkOrder)
                .AsQueryable();
            if (woId.HasValue) q = q.Where(t => t.WoId == woId.Value);

            var result = await q.OrderByDescending(t => t.Id).Select(t => new
            {
                t.Id,
                t.WoId,
                WoNumber = t.WorkOrder!.WoNumber,
                t.MaterialId,
                MaterialName = t.Material!.Name,
                MaterialCode = t.Material!.ItemCode,
                t.Type,
                t.Qty,
                t.OperatorName,
                t.CreatedAt
            }).ToListAsync();

            return Ok(result);
        }

        // 單一料號手動領料：扣庫存 + 寫入異動紀錄（交易保護，避免超領）
        [HttpPost("issue")]
        public async Task<IActionResult> Issue(IssueRequestDto dto)
        {
            if (dto.Qty <= 0) return BadRequest("領料數量必須大於 0");

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var wo = await _db.WorkOrders.FindAsync(dto.WoId);
                if (wo == null) return NotFound("工單不存在");

                var material = await _db.Materials.FindAsync(dto.MaterialId);
                if (material == null) return NotFound("物料不存在");

                if (material.StockQty < dto.Qty)
                    return BadRequest($"庫存不足：{material.Name} 現有 {material.StockQty}，需求 {dto.Qty}");

                material.StockQty -= dto.Qty;

                var record = new InventoryTransaction
                {
                    WoId = dto.WoId,
                    MaterialId = dto.MaterialId,
                    Type = "ISSUE",
                    Qty = dto.Qty,
                    OperatorName = dto.OperatorName,
                    CreatedAt = DateTime.Now
                };
                _db.InventoryTransactions.Add(record);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(record);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // 依工單所需產品的 BOM，一次展開並全部領料（先檢查所有料件都足夠才真正扣帳）
        [HttpPost("auto-issue")]
        public async Task<IActionResult> AutoIssue(AutoIssueRequestDto dto)
        {
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var wo = await _db.WorkOrders.FindAsync(dto.WoId);
                if (wo == null) return NotFound("工單不存在");

                if (wo.Status != "DRAFT")
                    return BadRequest($"工單狀態為 {wo.Status}，不能重複領料");

                var bomLines = await _db.Boms
                    .Where(b => b.ParentItemId == wo.ProductId)
                    .Include(b => b.ChildItem)
                    .ToListAsync();

                if (!bomLines.Any()) return BadRequest("此產品尚未設定 BOM");

                // 先全部檢查，任一料不足就整批擋下（避免部分扣帳）
                var shortages = bomLines
                    .Select(b => new
                    {
                        b.ChildItem!.Name,
                        Required = b.RequiredQty * wo.TargetQty,
                        b.ChildItem!.StockQty
                    })
                    .Where(x => x.StockQty < x.Required)
                    .ToList();

                if (shortages.Any())
                {
                    var msg = string.Join("; ", shortages.Select(s => $"{s.Name} 缺料 {s.Required - s.StockQty}"));
                    return BadRequest($"庫存不足，無法領料：{msg}");
                }

                var records = new List<InventoryTransaction>();
                foreach (var b in bomLines)
                {
                    var requiredTotal = (int)(b.RequiredQty * wo.TargetQty);
                    b.ChildItem!.StockQty -= requiredTotal;

                    var record = new InventoryTransaction
                    {
                        WoId = wo.Id,
                        MaterialId = b.ChildItemId,
                        Type = "ISSUE",
                        Qty = requiredTotal,
                        OperatorName = dto.OperatorName,
                        CreatedAt = DateTime.Now
                    };
                    records.Add(record);
                    _db.InventoryTransactions.Add(record);
                }

                wo.Status = "ISSUED"; // 領料成功後自動切換工單狀態

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(records);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // 成品入庫（完工）：增加成品庫存 + 寫入 RECEIPT 紀錄
        [HttpPost("receipt")]
        public async Task<IActionResult> Receipt(IssueRequestDto dto)
        {
            if (dto.Qty <= 0) return BadRequest("入庫數量必須大於 0");

            var material = await _db.Materials.FindAsync(dto.MaterialId);
            if (material == null) return NotFound("物料不存在");

            var wo = await _db.WorkOrders.FindAsync(dto.WoId);
            if (wo == null) return NotFound("工單不存在");

            material.StockQty += dto.Qty;
            wo.Status = "COMPLETED"; // 成品入庫代表工單完工

            var record = new InventoryTransaction
            {
                WoId = dto.WoId,
                MaterialId = dto.MaterialId,
                Type = "RECEIPT",
                Qty = dto.Qty,
                OperatorName = dto.OperatorName,
                CreatedAt = DateTime.Now
            };
            _db.InventoryTransactions.Add(record);
            await _db.SaveChangesAsync();

            return Ok(record);
        }
    }
}
