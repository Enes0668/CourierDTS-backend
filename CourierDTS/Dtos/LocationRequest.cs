namespace CourierDTS.Dtos
{
    // POST /api/locations, PUT /api/locations/{id}
    public class LocationRequest
    {
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
    }
}
