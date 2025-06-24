
namespace FlutterStart.Application.Exceptions
{
    // Exceção lançada quando o usuário não tem permissão para acessar o recurso.
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
