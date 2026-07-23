namespace CourierDTS.Models
{
    // Locations
    // ContactPerson/ContactPhone API sözleşmesinde yok ama eski tblTransferNoktaları
    // PDF'inden harmanlandı - hastane/depo yetkilisine ulaşım için tutulmaya devam ediliyor.
    public class Location : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
    }
}
