namespace AuthService.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(Guid userId, string email, IEnumerable<string>? roles = null);
        
    }
}
