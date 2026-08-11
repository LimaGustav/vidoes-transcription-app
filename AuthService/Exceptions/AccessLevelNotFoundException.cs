namespace AuthService.Exceptions
{
    public class AccessLevelNotFoundException : AppException
    {
        public AccessLevelNotFoundException(string accessLevelName)
            : base($"Nível de acesso '{accessLevelName}' não foi encontrado. Verifique se o seed foi executado.", StatusCodes.Status500InternalServerError)
        {
        }
    }
}
