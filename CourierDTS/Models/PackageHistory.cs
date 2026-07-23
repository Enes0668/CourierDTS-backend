namespace CourierDTS.Models
{
    // PackageHistories - "POST /api/packages/sync-actions" ile gelen her eylemin kaydı.
    // KRİTİK KURAL: bu tablodan hiçbir satır silinmemeli (soft-delete bile yapılmamalı) -
    // paketin hareketi kanunlar gereği kalıcı loglanmak zorunda (Chain of Custody).
    public class PackageHistory : BaseEntity
    {
        public int JourneyId { get; set; }
        public Journey Journey { get; set; } = null!;

        public int PackageId { get; set; }
        public Package Package { get; set; } = null!;

        public PackageActionType ActionType { get; set; }
        public DateTime ActionTime { get; set; }
        public string? Notes { get; set; }
    }
}
