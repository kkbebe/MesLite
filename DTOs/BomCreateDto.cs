namespace MesLite.DTOs
{
    public class BomCreateDto
    {
        public int ParentItemId { get; set; }
        public int ChildItemId { get; set; }
        public decimal RequiredQty { get; set; }
    }
}