using AuthService.Domains;
using AuthService.DTOs;
using AuthService.Exceptions;
using AuthService.Interfaces;
using System.Security.Authentication;

namespace AuthService.Application.Services
{
    public class AuthServiceHandler : IAuthService
    {
        private const string DefaultAccessLevel = "Viewer";

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthServiceHandler(
            IUserRepository userRepository,
            IPasswordHasherService passwordHasher,
            IJwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var emailExists = await _userRepository.EmailExistsAsync(request.Email);
            if (emailExists)
                throw new EmailAlreadyExistsException(request.Email);

            var accessLevel = await _userRepository.GetAccessLevelByNameAsync(DefaultAccessLevel);
            if (accessLevel is null)
                throw new AccessLevelNotFoundException(DefaultAccessLevel);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                AccessLevelId = accessLevel.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(user);

            return new RegisterResponse(
                createdUser.Id,
                createdUser.Email,
                accessLevel.Name,
                createdUser.CreatedAt
            );
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new InvalidCredentialsException();

            if (!user.IsActive)
                throw new UserInactiveException(user.Email);

            var token = _jwtTokenService.GenerateToken(user.Id, user.Email, new[] { user.AccessLevel.Name });

            return new LoginResponse(
                token,
                DateTime.UtcNow.AddMinutes(60), // idealmente vem do JwtSettings.ExpiresInMinutes, ver nota abaixo
                user.Id,
                user.Email,
                user.AccessLevel.Name
            );
        }
    }
}
