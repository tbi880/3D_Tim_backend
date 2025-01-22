using _3D_Tim_backend.Data;
using _3D_Tim_backend.Entities;
using Microsoft.EntityFrameworkCore;
using _3D_Tim_backend.Enums;

namespace _3D_Tim_backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task DeleteAllAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Users");
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByEmailAndPasswordAsync(string email, string password)
        {
            User? user = await GetByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLastVisitAtAsync(DateTime lastVisitAt, User user)
        {
            {
                user.LastVisitAt = lastVisitAt;
                await this.UpdateUserAsync(user);
            }
        }

        public async Task CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllTempUsersAsync()
        {
            return await _context.Users.Where(user => user.Role == Role.Guest).ToListAsync();
        }

        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
