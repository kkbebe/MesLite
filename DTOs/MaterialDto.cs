namespace MesLite.DTOs
{
    public class MaterialCreateDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "RAW";
        public int StockQty { get; set; } = 0;
    }

    public class MaterialUpdateDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "RAW";
        public int StockQty { get; set; }
    }
}