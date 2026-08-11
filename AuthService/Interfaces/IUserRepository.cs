using AuthService.Domains;

namespace AuthService.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<AccessLevel?> GetAccessLevelByNameAsync(string name);
        Task<User> AddAsync(User user);
    }
}
