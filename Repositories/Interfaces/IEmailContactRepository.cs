using _3D_Tim_backend.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _3D_Tim_backend.Repositories
{
    public interface IEmailContactRepository
    {
        Task<IEnumerable<EmailContact>> GetAllAsync();
        Task<EmailContact?> GetByIdAsync(int id);
        Task<EmailContact?> GetByNameAndEmailAsync(string name, string email);
        Task AddAsync(EmailContact emailContact);

        Task<string> GenerateUniqueVCodeAsync();

        Task<EmailContact?> GetByVCodeAsync(string vCode);

        Task DeleteAll();
        Task SaveChangesAsync();
    }
}
