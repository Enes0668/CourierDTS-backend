namespace CourierDTS.Dtos
{
    // PUT /api/packages/{id}
    public class UpdatePackageRequest
    {
        public string? Barcode { get; set; }
        public string Description { get; set; } = string.Empty;
        public short Priority { get; set; }
        public int PickupLocationId { get; set; }
        public int DropoffLocationId { get; set; }
    }
}
