using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MesLite.Models
{
    public class WorkOrder
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string WoNumber { get; set; } = string.Empty;

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Material? Product { get; set; }

        public int TargetQty { get; set; }

        // DRAFT 草稿 / ISSUED 已領料 / COMPLETED 已完工
        [Required, MaxLength(20)]
        public string Status { get; set; } = "DRAFT";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<InventoryTransaction>? Transactions { get; set; }
    }
}
