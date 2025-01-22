using System.Collections.Concurrent;

namespace _3D_Tim_backend.Services
{
    public class SessionManager : ISessionManager
    {
        private readonly ConcurrentDictionary<string, string> _activeSessions = new();

        public async Task AddSession(string email, string token)
        {
            _activeSessions[email] = token;
        }

        public async Task RemoveSession(string email)
        {
            _activeSessions.TryRemove(email, out _);
        }

        public async Task<bool> IsUserLoggedIn(string email)
        {
            return _activeSessions.ContainsKey(email);
        }

        public async Task<string> GetToken(string email)
        {
            _activeSessions.TryGetValue(email, out var token);
            return token;
        }
    }
}
