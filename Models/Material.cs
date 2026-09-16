using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MesLite.Models
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // RAW: 原物料 / FG: 成品
        [Required, MaxLength(10)]
        public string Type { get; set; } = "RAW";

        public int StockQty { get; set; } = 0;

        // Navigation
        public ICollection<Bom>? AsParentBoms { get; set; }
        public ICollection<Bom>? AsChildBoms { get; set; }
    }
}
