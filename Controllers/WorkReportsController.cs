using MesLite.Data;
using MesLite.DTOs;
using MesLite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MesLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkReportsController : ControllerBase
    {
        private readonly MesLiteContext _db;
        public WorkReportsController(MesLiteContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? woId)
        {
            var q = _db.WorkReports.Include(r => r.WorkOrder).AsQueryable();
            if (woId.HasValue) q = q.Where(r => r.WoId == woId.Value);

            var result = await q.OrderByDescending(r => r.Id).Select(r => new
            {
                r.Id,
                r.WoId,
                WoNumber = r.WorkOrder!.WoNumber,
                r.OperatorName,
                r.CompletedQty,
                r.ReportTime
            }).ToListAsync();

            return Ok(result);
        }

        // 新增一筆報工：工單必須是 ISSUED 狀態才能報工
        // 累計報工數量達到工單目標數量時，自動把工單狀態切成 COMPLETED
        [HttpPost]
        public async Task<IActionResult> Create(AddWorkReportDto dto)
        {
            if (dto.CompletedQty <= 0) return BadRequest("完工數量必須大於 0");

            var wo = await _db.WorkOrders.FindAsync(dto.WoId);
            if (wo == null) return NotFound("工單不存在");

            if (wo.Status != "ISSUED")
                return BadRequest($"工單狀態為 {wo.Status}，必須先完成領料 (ISSUED) 才能報工");

            var report = new WorkReport
            {
                WoId = dto.WoId,
                OperatorName = dto.OperatorName,
                CompletedQty = dto.CompletedQty,
                ReportTime = DateTime.Now
            };
            _db.WorkReports.Add(report);
            await _db.SaveChangesAsync();

            var totalReported = await _db.WorkReports
                .Where(r => r.WoId == wo.Id)
                .SumAsync(r => r.CompletedQty);

            if (totalReported >= wo.TargetQty)
            {
                wo.Status = "COMPLETED";
                await _db.SaveChangesAsync();
            }

            return Ok(new { report, totalReported, woStatus = wo.Status });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _db.WorkReports.FindAsync(id);
            if (r == null) return NotFound();
            _db.WorkReports.Remove(r);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
