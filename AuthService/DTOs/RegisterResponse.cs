namespace AuthService.DTOs
{
    public record RegisterResponse(
    Guid Id,
    string Email,
    string AccessLevel,
    DateTime CreatedAt
);
}
