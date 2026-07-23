namespace CourierDTS.Models
{
    // Journeys.Status
    public enum JourneyStatus
    {
        InProgress,
        Completed,
        Cancelled
    }

    // Packages.Status - sequence diyagramındaki "201 Created (Status: Pending)" ile başlıyor.
    public enum PackageStatus
    {
        Pending,
        PickedUp,
        InTransit,
        Delivered,
        Cancelled
    }

    // PackageHistories.ActionType - /api/packages/sync-actions içindeki actionType değerleri.
    public enum PackageActionType
    {
        PickedUp,
        InTransit,
        Delivered,
        Cancelled
    }
}
