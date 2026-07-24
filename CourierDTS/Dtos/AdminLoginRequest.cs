namespace CourierDTS.Dtos
{
    // POST /api/admin/login
    public class AdminLoginRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
