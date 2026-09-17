using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MesLite.Data;
using MesLite.Models;
using MesLite.DTOs;


namespace MesLite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly MesLiteContext _db;
        public MaterialsController(MesLiteContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Material>>> GetAll([FromQuery] string? type)
        {
            var q = _db.Materials.AsQueryable();
            if (!string.IsNullOrEmpty(type)) q = q.Where(m => m.Type == type);
            return await q.OrderBy(m => m.Id).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Material>> GetById(int id)
        {
            var m = await _db.Materials.FindAsync(id);
            if (m == null) return NotFound();
            return m;
        }

        [HttpPost]
        public async Task<ActionResult<Material>> Create(MaterialCreateDto dto)
        {
            var material = new Material
            {
                ItemCode = dto.ItemCode,
                Name = dto.Name,
                Type = dto.Type,
                StockQty = dto.StockQty
            };

            _db.Materials.Add(material);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = material.Id }, material);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MaterialUpdateDto dto)
        {
            var material = await _db.Materials.FindAsync(id);
            if (material == null) return NotFound();

            material.ItemCode = dto.ItemCode;
            material.Name = dto.Name;
            material.Type = dto.Type;
            material.StockQty = dto.StockQty;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _db.Materials.FindAsync(id);
            if (m == null) return NotFound();
            _db.Materials.Remove(m);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
