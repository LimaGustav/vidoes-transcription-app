using AuthService.Interfaces;
using BCrypt.Net;

namespace SeuProjeto.Infrastructure.Security;



public class PasswordHasherService : IPasswordHasherService
{
    private const int WorkFactor = 12; // custo computacional, 12 é um bom padrão em 2026

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);

    public bool Verify(string password, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}