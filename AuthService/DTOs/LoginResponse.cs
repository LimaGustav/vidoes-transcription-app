namespace AuthService.DTOs
{
    public record LoginResponse(
    string Token,
    DateTime ExpiresAt,
    Guid UserId,
    string Email,
    string AccessLevel
);
}
