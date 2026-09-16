using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MesLite.Models
{
    public class WorkReport
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(WorkOrder))]
        public int WoId { get; set; }
        public WorkOrder? WorkOrder { get; set; }

        [Required, MaxLength(50)]
        public string OperatorName { get; set; } = string.Empty;

        // 這次報工完成的數量
        public int CompletedQty { get; set; }

        public DateTime ReportTime { get; set; } = DateTime.Now;
    }
}
