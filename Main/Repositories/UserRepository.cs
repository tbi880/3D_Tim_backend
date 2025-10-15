using _3D_Tim_backend.Data;
using _3D_Tim_backend.Entities;
using Microsoft.EntityFrameworkCore;
using _3D_Tim_backend.Enums;
using _3D_Tim_backend.Exceptions;
using Microsoft.Extensions.Logging;

namespace _3D_Tim_backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task DeleteAllAsync()
        {
            _logger.LogInformation("Deleting all users");
            var allUsers = await _context.Users.ToListAsync();
            _context.Users.RemoveRange(allUsers);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            _logger.LogInformation("Getting all users");
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByEmailAndPasswordAsync(string email, string password)
        {
            _logger.LogInformation("Getting user by email and password {Email}", email);
            User? user = await GetByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.LogInformation("Getting user by email {Email}", email);
            return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Getting user by id {Id}", id);
            return await _context.Users.FindAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            _logger.LogInformation("Saving changes");
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLastVisitAtAsync(DateTime lastVisitAt, User user)
        {
            _logger.LogInformation("Updating last visit for user {UserId}", user.Id);
            user.LastVisitAt = lastVisitAt;
            await this.UpdateUserAsync(user);
        }

        public async Task CreateUserAsync(User user)
        {
            _logger.LogInformation("Creating user {Email}", user.Email);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _logger.LogInformation("Updating user {UserId}", user.Id);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllTempUsersAsync()
        {
            _logger.LogInformation("Getting all temp users");
            return await _context.Users.Where(user => user.Role == Role.Guest).ToListAsync();
        }

        public async Task DeleteUserAsync(User user)
        {
            _logger.LogInformation("Deleting user {UserId}", user.Id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task SyncUserDataToDbAsync<T>(T userDomainModel)
        {
            _logger.LogInformation("Syncing user data to database");
            var domainUser = userDomainModel as _3D_Tim_backend.Domain.IUser;
            if (domainUser == null)
            {
                throw new InvalidCastException("Invalid user domain model while syncing user data to db");
            }

            var user = await GetByIdAsync(domainUser.UserId);
            if (user == null)
            {
                throw new UserNotFoundException(domainUser.UserId);
            }
            user.Money += domainUser.MoneyInRoom;
            user.LastVisitAt = DateTime.Now;
            user.TotalBets += domainUser.TotalBetsInRoom;

            await UpdateUserAsync(user);
        }
    }
}
