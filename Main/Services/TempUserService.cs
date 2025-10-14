using _3D_Tim_backend.DTOs;
using _3D_Tim_backend.Entities;
using _3D_Tim_backend.Repositories;
using _3D_Tim_backend.Enums;
using Microsoft.Extensions.Logging;


public class TempUserService : ITempUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TempUserService> _logger;

    public TempUserService(IUserRepository userRepository, ILogger<TempUserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User?> CreateTempUserAsync<T>(T tempUserRegisterDTO)
    {
        _logger.LogInformation("Creating temporary user");
        TempUserRegisterDTO? tempUser = tempUserRegisterDTO as TempUserRegisterDTO;
        if (tempUser.Email == null || tempUser.Name == null)
        {
            throw new Exception("Email or Name is null");
        }
        if (!tempUser.Email.Contains("@"))
        {
            throw new Exception("Invalid email");
        }
        if (tempUser.Name.Length < 2)
        {
            throw new Exception("Name must be at least 2 characters long");
        }

        User? userInDB = await _userRepository.GetByEmailAsync(tempUser.Email.ToLower());
        if (userInDB != null)
        {
            if (userInDB.Role != Role.Guest)
            {
                throw new Exception("User already exists as a registered user");
            }
            else
            {
                _logger.LogInformation("Deleting existing guest user {Email}", userInDB.Email.ToLower());
                await _userRepository.DeleteUserAsync(userInDB);
            }
        }
        User user = new User
        {
            Name = tempUser.Name,
            Email = tempUser.Email,
        };

        _logger.LogInformation("Temporary user {Email} created", user.Email);
        await _userRepository.CreateUserAsync(user);
        await _userRepository.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAllOutDatedTempUsersAsync()
    {
        _logger.LogInformation("Deleting outdated temporary users");
        List<User> tempUsers = [.. await _userRepository.GetAllTempUsersAsync()];
        DateTime now = DateTime.Now;
        foreach (User user in tempUsers)
        {
            if (user.LastVisitAt.HasValue && user.LastVisitAt.Value.AddDays(3) < now)
            {
                _logger.LogInformation("Removing user {Email}", user.Email);
                await _userRepository.DeleteUserAsync(user);
            }
        }

    }

    public async Task<IEnumerable<User>> GetAllTempUserAsync()
    {
        _logger.LogInformation("Retrieving all temporary users");
        return await _userRepository.GetAllTempUsersAsync();
    }

}