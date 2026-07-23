namespace CourierDTS.Models
{
    // Journeys - kuryenin "POST /api/journeys/start" ile başlattığı tek bir sefer.
    public class Journey : BaseEntity
    {
        public int CourierId { get; set; }
        public Courier Courier { get; set; } = null!;

        public int StartLocId { get; set; }
        public Location StartLocation { get; set; } = null!;

        public int EndLocId { get; set; }
        public Location EndLocation { get; set; } = null!;

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public JourneyStatus Status { get; set; } = JourneyStatus.InProgress;
    }
}
