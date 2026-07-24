namespace CourierDTS.Dtos
{
    // POST /api/couriers
    public class CreateCourierRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public char Sex { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Phone { get; set; } = string.Empty;
        public int? ActiveVehicleId { get; set; }
    }
}
