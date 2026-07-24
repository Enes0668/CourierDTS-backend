using System.Collections.Concurrent;

namespace CourierDTS.Auth
{
    // İlkel oturum yönetimi - gerçek auth (Azure AD) gelince kaldırılacak.
    // Bellekte tutuluyor: uygulama yeniden başlarsa (Render'da deploy/uyku sonrası)
    // tüm oturumlar sıfırlanır, kullanıcı tekrar giriş yapmak zorunda kalır.
    public static class SessionStore
    {
        public record Session(string Role, int UserId);

        private static readonly ConcurrentDictionary<string, Session> _sessions = new();

        public static string CreateSession(string role, int userId)
        {
            var token = Guid.NewGuid().ToString();
            _sessions[token] = new Session(role, userId);
            return token;
        }

        public static bool IsValidForRole(string? token, string role)
        {
            if (token == null)
                return false;

            return _sessions.TryGetValue(token, out var session) && session.Role == role;
        }
    }
}
