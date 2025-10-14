using _3D_Tim_backend.DTOs;
using _3D_Tim_backend.Entities;
using _3D_Tim_backend.Repositories;
using Microsoft.Extensions.Logging;


public class EmailContactService : IEmailContactService
{
    private readonly IEmailContactRepository _emailContactRepository;
    private readonly ILogger<EmailContactService> _logger;

    public EmailContactService(IEmailContactRepository emailContactRepository, IMessageQueueService messageQueueService, ILogger<EmailContactService> logger)
    {
        _emailContactRepository = emailContactRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<EmailContact>> GetAllContactsAsync()
    {
        _logger.LogInformation("Fetching all email contacts");
        return await _emailContactRepository.GetAllAsync();
    }

    public async Task<EmailContact?> GetContactByEmailAsync(string email)
    {
        _logger.LogInformation("Fetching contact by email {Email}", email);
        return await _emailContactRepository.GetByEmailAsync(email);
    }

    public async Task CreateOrUpdateEmailContactAsync<T>(T dto)
    {
        _logger.LogInformation("Creating or updating email contact");
        if (dto is not CreateEmailContactDto emailContactDto)
        {
            throw new ArgumentException("The provided type does not contain the required properties.");
        }

        if (string.IsNullOrEmpty(emailContactDto.Email) || string.IsNullOrEmpty(emailContactDto.Name) || string.IsNullOrEmpty(emailContactDto.Message))
        {
            throw new ArgumentException("Email, Name, and Message cannot be null or empty.");
        }

        if (!emailContactDto.Email.Contains("@"))
        {
            throw new ArgumentException("Invalid email format.");
        }

        var emailContactInDb = await _emailContactRepository.GetByEmailAsync(emailContactDto.Email.ToLower());

        if (emailContactInDb != null)
        {
            emailContactInDb.Name = emailContactDto.Name;
            emailContactInDb.Message = emailContactDto.Message;
            emailContactInDb.AllowSaveEmail = emailContactDto.AllowSaveEmail;
            _logger.LogInformation("Updating existing contact {Email}", emailContactDto.Email);
            await _emailContactRepository.UpdateContactAsync(emailContactInDb);
            return;
        }

        var newEmailContact = new EmailContact
        {
            Name = emailContactDto.Name,
            Email = emailContactDto.Email.ToLower(),
            Message = emailContactDto.Message,
            AllowSaveEmail = emailContactDto.AllowSaveEmail,
            VCode = await _emailContactRepository.GenerateUniqueVCodeAsync()
        };
        _logger.LogInformation("Adding new contact {Email}", newEmailContact.Email);
        await _emailContactRepository.AddAsync(newEmailContact);
        await _emailContactRepository.SaveChangesAsync();
        return;
    }


    public async Task<EmailContact?> VerifyContactByVCodeAsync(string vCode)
    {
        _logger.LogInformation("Verifying contact by VCode");
        return await _emailContactRepository.GetByVCodeAsync(vCode);
    }

    public async Task UpdateContactVerifiedAtAsync(EmailContact emailContact)
    {
        _logger.LogInformation("Updating verified timestamp for {Email}", emailContact.Email);
        await _emailContactRepository.UpdateVerifiedAtAsync(DateTime.Now, emailContact);
    }

    public async Task DeleteAllContactsAsync()
    {
        _logger.LogInformation("Deleting all email contacts");
        await _emailContactRepository.DeleteAllAsync();
    }

}