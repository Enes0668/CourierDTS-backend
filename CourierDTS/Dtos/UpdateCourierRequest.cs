namespace CourierDTS.Dtos
{
    // PUT /api/couriers/{id}
    public class UpdateCourierRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public char Sex { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Phone { get; set; } = string.Empty;
        public int? ActiveVehicleId { get; set; }
    }
}
