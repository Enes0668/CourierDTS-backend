namespace CourierDTS.Models
{
    // Vehicles
    // Not: Courier.ActiveVehicleId ile birlikte iki yönlü bir ilişki oluşuyor -
    // CourierId burada "bu aracın sahibi/sürücüsü kim", Courier.ActiveVehicleId ise
    // "bu kurye şu an hangi aracı kullanıyor" sorusunu cevaplıyor. İkisi de sözleşmede
    // ayrı ayrı FK olarak listelenmiş, bilerek ikisi de tutuldu.
    // Bilerek hız (speed) bilgisi yok - "araç hız bilgisi olmayacak" denilmişti.
    public class Vehicle : BaseEntity
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;

        public int? CourierId { get; set; }
        public Courier? Courier { get; set; }
    }
}
