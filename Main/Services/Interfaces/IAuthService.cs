public interface IAuthService
{
    Task RegisterAsync<T>(T userRegisterDTO);
    Task<bool> IsUserLoggedInAsync(string email);
    Task LogoutAsync<T>(T email);
    Task<string?> LoginAsync<T>(T userLoginDTO);
    Task<string?> RegisterGuestUserAsync<T>(T tempUserRegisterDTO);

    Task DeleteAllAccountAsync();

}
