namespace FlutterStart.Application.Exceptions
{
    // Exceção lançada quando o usuário não está autenticado (ex: token ausente ou inválido).
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}
