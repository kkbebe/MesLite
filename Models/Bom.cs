using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MesLite.Models
{
    public class Bom
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(ParentItem))]
        public int ParentItemId { get; set; }
        public Material? ParentItem { get; set; }

        [ForeignKey(nameof(ChildItem))]
        public int ChildItemId { get; set; }
        public Material? ChildItem { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal RequiredQty { get; set; }
    }
}
