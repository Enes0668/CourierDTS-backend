namespace CourierDTS.Models
{
    // Packages - "POST /api/packages" ile yönetici tarafından oluşturulan tıbbi numune/paket.
    public class Package : BaseEntity
    {
        public string? Barcode { get; set; }
        public string Description { get; set; } = string.Empty;
        public short Priority { get; set; }

        public int PickupLocId { get; set; }
        public Location PickupLocation { get; set; } = null!;

        public int DropoffLocId { get; set; }
        public Location DropoffLocation { get; set; } = null!;

        // Nullable: paket henüz bir kuryeye atanmadan da oluşturulabilir.
        public int? AssignedCourierId { get; set; }
        public Courier? AssignedCourier { get; set; }

        public PackageStatus Status { get; set; } = PackageStatus.Pending;

        public void Cancel()
        {
            if (Status == PackageStatus.Delivered)
                throw new InvalidOperationException("Teslim edilmiş bir paket iptal edilemez.");

            Status = PackageStatus.Cancelled;
        }
    }
}
