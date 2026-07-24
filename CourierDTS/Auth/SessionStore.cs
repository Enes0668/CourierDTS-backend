using System.Collections.Concurrent;

namespace CourierDTS.Auth
{
    // İlkel oturum yönetimi - gerçek auth (Azure AD) gelince kaldırılacak.
    // Bellekte tutuluyor: uygulama yeniden başlarsa (Render'da deploy/uyku sonrası)
    // tüm oturumlar sıfırlanır, kullanıcı tekrar giriş yapmak zorunda kalır.
    public static class SessionStore
    {
        private static readonly ConcurrentDictionary<string, int> _tokenToAdminId = new();

        public static string CreateSession(int adminId)
        {
            var token = Guid.NewGuid().ToString();
            _tokenToAdminId[token] = adminId;
            return token;
        }

        public static bool IsValid(string? token)
        {
            return token != null && _tokenToAdminId.ContainsKey(token);
        }
    }
}
