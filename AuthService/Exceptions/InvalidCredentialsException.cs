namespace AuthService.Exceptions
{
    public class InvalidCredentialsException : AppException
    {
        public InvalidCredentialsException()
            : base("Email ou senha inválidos.", StatusCodes.Status401Unauthorized)
        {
        }
    }
}
