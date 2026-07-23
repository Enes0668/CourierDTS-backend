namespace CourierDTS.Dtos
{
    // POST /api/packages
    public class CreatePackageRequest
    {
        public string? Barcode { get; set; }
        public string Description { get; set; } = string.Empty;
        public short Priority { get; set; }
        public int PickupLocationId { get; set; }
        public int DropoffLocationId { get; set; }
        public int? AssignedCourierId { get; set; }
    }
}
