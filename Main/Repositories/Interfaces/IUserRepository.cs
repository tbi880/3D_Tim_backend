using _3D_Tim_backend.Entities;

namespace _3D_Tim_backend.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAndPasswordAsync(string email, string password);

        Task<User?> GetByEmailAsync(string email);

        Task<IEnumerable<User>> GetAllTempUsersAsync();

        Task UpdateUserAsync(User user);

        Task SyncUserDataToDbAsync<T>(T userDomainModel);

        Task UpdateLastVisitAtAsync(DateTime lastVisitAt, User user);


        Task CreateUserAsync(User user);

        Task DeleteAllAsync();

        Task DeleteUserAsync(User user);

        Task SaveChangesAsync();
    }
}
