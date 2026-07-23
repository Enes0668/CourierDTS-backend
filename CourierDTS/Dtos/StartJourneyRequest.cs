namespace CourierDTS.Dtos
{
    // POST /api/journeys/start
    public class StartJourneyRequest
    {
        public int CourierId { get; set; }
        public int StartLocationId { get; set; }
        public int EndLocationId { get; set; }
    }
}
