using _3D_Tim_backend.Entities;

public interface ITempUserService
{
    Task<IEnumerable<User>> GetAllTempUserAsync();

    Task<User?> CreateTempUserAsync<T>(T tempUserRegisterDTO);

    Task DeleteAllOutDatedTempUsersAsync();
}
