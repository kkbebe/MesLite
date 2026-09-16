using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MesLite.Data;
using MesLite.Models;

namespace MesLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BomController : ControllerBase
    {
        private readonly MesLiteContext _db;
        public BomController(MesLiteContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? parentItemId)
        {
            var q = _db.Boms
                .Include(b => b.ParentItem)
                .Include(b => b.ChildItem)
                .AsQueryable();
            if (parentItemId.HasValue) q = q.Where(b => b.ParentItemId == parentItemId.Value);

            var result = await q.Select(b => new
            {
                b.Id,
                b.ParentItemId,
                ParentItemName = b.ParentItem!.Name,
                b.ChildItemId,
                ChildItemName = b.ChildItem!.Name,
                ChildItemCode = b.ChildItem!.ItemCode,
                b.RequiredQty
            }).ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Bom>> Create(Bom bom)
        {
            _db.Boms.Add(bom);
            await _db.SaveChangesAsync();
            return Ok(bom);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var b = await _db.Boms.FindAsync(id);
            if (b == null) return NotFound();
            _db.Boms.Remove(b);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 展開某成品的完整用料需求（依 target qty 計算）
        [HttpGet("explode/{productId}/{targetQty}")]
        public async Task<ActionResult<IEnumerable<object>>> Explode(int productId, int targetQty)
        {
            var lines = await _db.Boms
                .Where(b => b.ParentItemId == productId)
                .Include(b => b.ChildItem)
                .Select(b => new
                {
                    MaterialId = b.ChildItemId,
                    ItemCode = b.ChildItem!.ItemCode,
                    Name = b.ChildItem!.Name,
                    CurrentStock = b.ChildItem!.StockQty,
                    RequiredPerUnit = b.RequiredQty,
                    RequiredTotal = b.RequiredQty * targetQty
                }).ToListAsync();

            return Ok(lines);
        }
    }
}
