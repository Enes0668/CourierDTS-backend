namespace CourierDTS.Models
{
    // TelemetryLogs - "POST /api/telemetry/batch" ile gelen ham GPS koordinatları.
    // long Id: her kuryeden 30 saniyede bir nokta geldiği için, diğer tablolara göre
    // çok daha hızlı büyüyecek - int'in sınırına (~2,1 milyar) takılmamak için.
    public class TelemetryLog : BaseEntity<long>
    {
        public int JourneyId { get; set; }
        public Journey Journey { get; set; } = null!;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
