namespace CourierDTS.Models
{
    // Web panelden kuryeleri izleyen, iptal/atama gibi işlemleri yapan yönetici.
    public class Admin : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // İlkel giriş kontrolü için - düz metin değil, hash olarak saklanıyor.
        // Gerçek auth (Azure AD) gelince bu alan/login endpoint'i kaldırılacak.
        public string PasswordHash { get; set; } = string.Empty;
    }
}
