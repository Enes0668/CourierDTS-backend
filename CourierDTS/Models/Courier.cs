namespace CourierDTS.Models
{
    // Couriers
    // Surname/Sex/DateOfBirth API sözleşmesinde yok ama eski tblKuryeler PDF'inden
    // harmanlandı - personel kaydı için tutulmaya devam ediliyor.
    public class Courier : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public char Sex { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Phone { get; set; } = string.Empty;
        // İstihdam durumu değil - kuryenin şu an vardiyada/online olup olmadığı.
        // Gün içinde sık değişir (uygulamayı açtığında/kapattığında). Not: şu an
        // bunu true/false yapan bir endpoint yok, bu da ayrıca eklenmesi gereken bir şey.
        public bool IsActive { get; set; } = false;

        // Kuryenin son bilinen konumu - canlı haritada her kurye için
        // ayrı TelemetryLog sorgusu atmadan hızlıca gösterebilmek için.
        public double? LastLat { get; set; }
        public double? LastLng { get; set; }

        public int? ActiveVehicleId { get; set; }
        public Vehicle? ActiveVehicle { get; set; }
    }
}
