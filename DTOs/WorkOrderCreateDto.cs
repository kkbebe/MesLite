namespace MesLite.DTOs
{
    public class WorkOrderCreateDto
    {
        public string WoNumber { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int TargetQty { get; set; }
    }
}