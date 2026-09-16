namespace MesLite.DTOs
{
    public class AddWorkReportDto
    {
        public int WoId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public int CompletedQty { get; set; }
    }
}