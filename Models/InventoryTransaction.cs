using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MesLite.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(WorkOrder))]
        public int WoId { get; set; }
        public WorkOrder? WorkOrder { get; set; }

        [ForeignKey(nameof(Material))]
        public int MaterialId { get; set; }
        public Material? Material { get; set; }

        // ISSUE 領料 / RECEIPT 成品入庫
        [Required, MaxLength(20)]
        public string Type { get; set; } = "ISSUE";

        public int Qty { get; set; }

        [MaxLength(50)]
        public string? OperatorName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
