namespace CourierDTS.Dtos
{
    // POST /api/courier/login
    public class CourierLoginRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
