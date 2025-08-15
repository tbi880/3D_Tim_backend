using _3D_Tim_backend.Data;
using _3D_Tim_backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace _3D_Tim_backend.Repositories
{
    public class EmailContactRepository : IEmailContactRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmailContactRepository> _logger;

        public EmailContactRepository(AppDbContext context, ILogger<EmailContactRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<EmailContact>> GetAllAsync()
        {
            _logger.LogInformation("Getting all email contacts");
            return await _context.EmailContacts.ToListAsync();
        }

        public async Task<EmailContact?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting contact by id {Id}", id);
            return await _context.EmailContacts.FindAsync(id);
        }

        public async Task<EmailContact?> GetByNameAndEmailAsync(string name, string email)
        {
            _logger.LogInformation("Getting contact by name {Name} and email {Email}", name, email);
            return await _context.EmailContacts.FirstOrDefaultAsync(contact => contact.Name == name && contact.Email == email);
        }


        public async Task AddAsync(EmailContact emailContact)
        {
            _logger.LogInformation("Adding email contact {Email}", emailContact.Email);
            await _context.EmailContacts.AddAsync(emailContact);
        }

        public async Task<string> GenerateUniqueVCodeAsync()
        {
            _logger.LogInformation("Generating unique verification code");
            string vCode;
            do
            {
                vCode = Generate8CharacterVCode();
            }
            while (await _context.EmailContacts.AnyAsync(ec => ec.VCode == vCode));
            return vCode;
        }

        private string Generate8CharacterVCode()
        {
            string chars = Guid.NewGuid().ToString("N");
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<EmailContact?> GetByVCodeAsync(string vCode)
        {
            _logger.LogInformation("Getting contact by VCode");
            return await _context.EmailContacts.FirstOrDefaultAsync(contact => contact.VCode == vCode);

        }


        public async Task DeleteAllAsync()
        {
            _logger.LogInformation("Deleting all email contacts");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM EmailContacts");
        }

        public async Task SaveChangesAsync()
        {
            _logger.LogInformation("Saving changes");
            await _context.SaveChangesAsync();
        }

        public async Task<EmailContact?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Getting contact by email {Email}", email);
            return await _context.EmailContacts.FirstOrDefaultAsync(contact => contact.Email == email);
        }

        public async Task UpdateContactAsync(EmailContact emailContact)
        {
            _logger.LogInformation("Updating contact {Email}", emailContact.Email);
            _context.EmailContacts.Update(emailContact);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVerifiedAtAsync(DateTime verifiedAt, EmailContact emailContact)
        {
            _logger.LogInformation("Updating verified at for {Email}", emailContact.Email);
            emailContact.VerifiedAt = verifiedAt;
            await this.UpdateContactAsync(emailContact);
        }
    }
}
