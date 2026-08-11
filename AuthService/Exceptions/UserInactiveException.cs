namespace AuthService.Exceptions
{
    public class UserInactiveException : AppException
    {
        public UserInactiveException(string email)
            : base($"O usuário '{email}' está inativo.", StatusCodes.Status403Forbidden)
        {
        }
    }
}
