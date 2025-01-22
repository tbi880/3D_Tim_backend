using _3D_Tim_backend.Entities;

public interface ISessionManager
{
    Task<string> GetToken(string email);

    Task<bool> IsUserLoggedIn(string email);

    Task AddSession(string email, string token);

    Task RemoveSession(string email);
}
