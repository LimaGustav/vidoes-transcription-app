namespace SeuProjeto.Infrastructure.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public int ExpiresInMinutes { get; set; } = 60;
}