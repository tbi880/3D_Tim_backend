using _3D_Tim_backend.DTOs;
using _3D_Tim_backend.Entities;
using _3D_Tim_backend.Repositories;
using _3D_Tim_backend.Enums;


public class TempUserService : ITempUserService
{
    private readonly IUserRepository _userRepository;

    public TempUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> CreateTempUserAsync<T>(T tempUserRegisterDTO)
    {
        TempUserRegisterDTO? tempUser = tempUserRegisterDTO as TempUserRegisterDTO;
        if (tempUser.Email == null || tempUser.Name == null)
        {
            throw new Exception("Email or Name is null");
        }
        User? userInDB = await _userRepository.GetByEmailAsync(tempUser.Email);
        if (userInDB != null)
        {
            if (userInDB.Role != Role.Guest)
            {
                throw new Exception("User already exists");
            }
            else
            {
                await _userRepository.DeleteUserAsync(userInDB);
            }
        }
        User user = new User
        {
            Name = tempUser.Name,
            Email = tempUser.Email,
        };

        await _userRepository.CreateUserAsync(user);
        await _userRepository.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAllOutDatedTempUsersAsync()
    {
        List<User> tempUsers = [.. await _userRepository.GetAllTempUsersAsync()];
        DateTime now = DateTime.Now;
        foreach (User user in tempUsers)
        {
            if (user.LastVisitAt.HasValue && user.LastVisitAt.Value.AddDays(3) < now)
            {
                await _userRepository.DeleteUserAsync(user);
            }
        }

    }

    public async Task<IEnumerable<User>> GetAllTempUserAsync()
    {
        return await _userRepository.GetAllTempUsersAsync();
    }

}