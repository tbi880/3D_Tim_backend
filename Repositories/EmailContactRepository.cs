using _3D_Tim_backend.Data;
using _3D_Tim_backend.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography.X509Certificates;

namespace _3D_Tim_backend.Repositories
{
    public class EmailContactRepository : IEmailContactRepository
    {
        private readonly AppDbContext _context;

        public EmailContactRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmailContact>> GetAllAsync()
        {
            return await _context.EmailContacts.ToListAsync();
        }

        public async Task<EmailContact?> GetByIdAsync(int id)
        {
            return await _context.EmailContacts.FindAsync(id);
        }

        public async Task<EmailContact?> GetByNameAndEmailAsync(string name, string email)
        {
            return await _context.EmailContacts.FirstOrDefaultAsync(contact => contact.Name == name && contact.Email == email);
        }


        public async Task AddAsync(EmailContact emailContact)
        {
            await _context.EmailContacts.AddAsync(emailContact);
        }

        public async Task<string> GenerateUniqueVCodeAsync()
        {
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
            return await _context.EmailContacts.FirstOrDefaultAsync(contact => contact.VCode == vCode);

        }


        public async Task DeleteAllAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM EmailContacts");
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<EmailContact?> GetByEmailAsync(string email)
        {
            return await _context.EmailContacts.FirstOrDefaultAsync(contact => contact.Email == email);
        }

        public async Task UpdateContactAsync(EmailContact emailContact)
        {
            _context.EmailContacts.Update(emailContact);
            await _context.SaveChangesAsync();
        }

    }
}
