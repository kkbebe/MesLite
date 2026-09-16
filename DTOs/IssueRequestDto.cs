namespace MesLite.DTOs
{
    // 手動指定領料（單一料號）
    public class IssueRequestDto
    {
        public int WoId { get; set; }
        public int MaterialId { get; set; }
        public int Qty { get; set; }
        public string? OperatorName { get; set; }
    }

    // 依工單 BOM 自動展開一次全部領料
    public class AutoIssueRequestDto
    {
        public int WoId { get; set; }
        public string? OperatorName { get; set; }
    }
}
