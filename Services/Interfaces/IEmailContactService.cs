using _3D_Tim_backend.Entities;

public interface IEmailContactService
{
    Task<IEnumerable<EmailContact>> GetAllContactsAsync();
    Task<EmailContact?> GetContactByEmailAsync(string email);
    Task CreateOrUpdateEmailContactAsync<T>(T emailContactDto);
    Task<EmailContact?> VerifyContactByVCodeAsync(string vCode);
    Task DeleteAllContactsAsync();
}
