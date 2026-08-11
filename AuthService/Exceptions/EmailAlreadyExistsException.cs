namespace AuthService.Exceptions
{
    public class EmailAlreadyExistsException : AppException
    {
        public EmailAlreadyExistsException(string email)
            : base($"O email '{email}' já está cadastrado.", StatusCodes.Status409Conflict)
        {
        }
    }
}
