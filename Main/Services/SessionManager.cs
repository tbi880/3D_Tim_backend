using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace _3D_Tim_backend.Services
{
    public class SessionManager : ISessionManager
    {
        private readonly ConcurrentDictionary<string, string> _activeSessions = new();
        private readonly ILogger<SessionManager> _logger;

        public SessionManager(ILogger<SessionManager> logger)
        {
            _logger = logger;
        }

        public async Task AddSession(string email, string token)
        {
            _logger.LogInformation("Adding session for {Email}", email);
            _activeSessions[email] = token;
        }

        public async Task RemoveSession(string email)
        {
            _logger.LogInformation("Removing session for {Email}", email);
            _activeSessions.TryRemove(email, out _);
        }

        public async Task<bool> IsUserLoggedIn(string email)
        {
            _logger.LogInformation("Checking session for {Email}", email);
            return _activeSessions.ContainsKey(email);
        }

        public async Task<string> GetToken(string email)
        {
            _logger.LogInformation("Retrieving token for {Email}", email);
            _activeSessions.TryGetValue(email, out var token);
            return token;
        }
    }
}
