using _3D_Tim_backend.DTOs;
using _3D_Tim_backend.Entities;
using _3D_Tim_backend.Repositories;
using _3D_Tim_backend.Enums;
using System.Collections.Concurrent;
using _3D_Tim_backend.Utils;
using Microsoft.AspNetCore.Http.HttpResults;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITempUserService _tempUserService;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly ISessionManager _sessionManager;

    public AuthService(IUserRepository userRepository, ITempUserService tempUserService, JwtTokenGenerator jwtTokenGenerator, ISessionManager sessionManager)
    {
        _userRepository = userRepository;
        _tempUserService = tempUserService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _sessionManager = sessionManager;
    }

    public async Task DeleteAllAccountAsync()
    {
        await _userRepository.DeleteAllAsync();
    }

    public async Task<bool> IsUserLoggedInAsync(string email)
    {
        return await _sessionManager.IsUserLoggedIn(email);
    }

    public async Task<string?> LoginAsync<T>(T userLoginDTO)
    {
        UserLoginDTO? loginDTO = userLoginDTO as UserLoginDTO;
        if (loginDTO.Email == null || loginDTO.Password == null)
        {
            throw new Exception("Email or Password is null");
        }
        var user = await _userRepository.GetByEmailAndPasswordAsync(loginDTO.Email, loginDTO.Password);
        if (user == null)
        {
            throw new Exception("Invalid Email or password");
        }
        var token = _jwtTokenGenerator.GenerateToken(user);
        await _sessionManager.AddSession(user.Email, token);
        await _userRepository.UpdateLastVisitAtAsync(DateTime.Now, user);
        return token;
    }

    public async Task LogoutAsync<T>(T email)
    {
        await _sessionManager.RemoveSession(email as string);
    }

    public async Task RegisterAsync<T>(T userRegisterDTO)
    {
        UserRegisterDTO? registerDTO = userRegisterDTO as UserRegisterDTO;
        if (registerDTO.Name == null || registerDTO.Email == null || registerDTO.Password == null)
        {
            throw new Exception("Name, Email or Password is null");
        }
        if (!registerDTO.Email.Contains("@"))
        {
            throw new Exception("Invalid email");
        }
        if (registerDTO.Password.Length < 8)
        {
            throw new Exception("Password must be at least 8 characters long");
        }
        User? userInDB = await _userRepository.GetByEmailAsync(registerDTO.Email);
        if (userInDB != null)
        {
            if (userInDB.Role != Role.Guest)
            {
                throw new Exception("User already exists");
            }
            else
            {
                userInDB.Name = registerDTO.Name;
                userInDB.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password);
                userInDB.Role = Role.User;
                await _userRepository.UpdateUserAsync(userInDB);
                await _userRepository.SaveChangesAsync();
                return;
            }
        }
        User user = new User
        {
            Name = registerDTO.Name,
            Email = registerDTO.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password),
            Role = Role.User
        };
        await _userRepository.CreateUserAsync(user);
        await _userRepository.SaveChangesAsync();

    }

    public async Task<string?> RegisterGuestUserAsync<T>(T tempUserRegisterDTO)
    {
        User? user;
        try
        {
            user = await _tempUserService.CreateTempUserAsync(tempUserRegisterDTO);
        }
        catch (Exception ex)
        {
            if (ex.Message == "User already exists as a registered user")
            {
                throw new Exception("User already exists as a registered user");
            }
            throw;
        }
        var token = _jwtTokenGenerator.GenerateToken(user);
        await _sessionManager.AddSession(user.Email, token);
        return token;
    }


}