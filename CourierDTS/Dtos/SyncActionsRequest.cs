using CourierDTS.Models;

namespace CourierDTS.Dtos
{
    // POST /api/packages/sync-actions
    public class SyncActionsRequest
    {
        public int CourierId { get; set; }
        public int JourneyId { get; set; }
        public List<SyncActionItem> Actions { get; set; } = new();
    }

    public class SyncActionItem
    {
        public int PackageId { get; set; }
        public PackageActionType ActionType { get; set; }
        public DateTime ActionTime { get; set; }
        public string? Notes { get; set; }
    }
}
