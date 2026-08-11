using AuthService.Domains;
using AuthService.Infrastructure.Persistence.Context;
using AuthService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users
                .Include(u => u.AccessLevel)
                .FirstOrDefaultAsync(u => u.Email == email);

        public async Task<bool> EmailExistsAsync(string email)
            => await _context.Users.AnyAsync(u => u.Email == email);

        public async Task<AccessLevel?> GetAccessLevelByNameAsync(string name)
            => await _context.AccessLevels
                .FirstOrDefaultAsync(a => a.Name == name);

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
