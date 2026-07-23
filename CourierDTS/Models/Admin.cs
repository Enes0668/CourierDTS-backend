namespace CourierDTS.Models
{
    // Web panelden kuryeleri izleyen, iptal/atama gibi işlemleri yapan yönetici.
    public class Admin : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
