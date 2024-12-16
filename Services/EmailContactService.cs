using _3D_Tim_backend.DTOs;
using _3D_Tim_backend.Entities;
using _3D_Tim_backend.Repositories;


public class EmailContactService : IEmailContactService
{
    private readonly IEmailContactRepository _emailContactRepository;
    public EmailContactService(IEmailContactRepository emailContactRepository, IMessageQueueService messageQueueService)
    {
        _emailContactRepository = emailContactRepository;
    }

    public async Task<IEnumerable<EmailContact>> GetAllContactsAsync()
    {
        return await _emailContactRepository.GetAllAsync();
    }

    public async Task<EmailContact?> GetContactByEmailAsync(string email)
    {
        return await _emailContactRepository.GetByEmailAsync(email);
    }

    public async Task CreateOrUpdateEmailContactAsync<T>(T dto)
    {
        if (dto is not CreateEmailContactDto emailContactDto)
        {
            throw new ArgumentException("The provided type does not contain the required properties.");
        }

        var emailContactInDb = await _emailContactRepository.GetByEmailAsync(emailContactDto.Email);

        if (emailContactInDb != null)
        {
            emailContactInDb.Name = emailContactDto.Name;
            emailContactInDb.Message = emailContactDto.Message;
            emailContactInDb.AllowSaveEmail = emailContactDto.AllowSaveEmail;
            await _emailContactRepository.UpdateContactAsync(emailContactInDb);
            return;
        }

        var newEmailContact = new EmailContact
        {
            Name = emailContactDto.Name,
            Email = emailContactDto.Email,
            Message = emailContactDto.Message,
            AllowSaveEmail = emailContactDto.AllowSaveEmail,
            VCode = await _emailContactRepository.GenerateUniqueVCodeAsync()
        };
        await _emailContactRepository.AddAsync(newEmailContact);
        await _emailContactRepository.SaveChangesAsync();
        return;
    }


    public async Task<EmailContact?> VerifyContactByVCodeAsync(string vCode)
    {
        return await _emailContactRepository.GetByVCodeAsync(vCode);
    }


    public async Task DeleteAllContactsAsync()
    {
        await _emailContactRepository.DeleteAllAsync();
    }


}