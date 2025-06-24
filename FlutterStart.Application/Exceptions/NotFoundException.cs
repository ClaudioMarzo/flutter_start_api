namespace FlutterStart.Application.Exceptions
{
    // Exceção lançada quando um recurso não é encontrado (ex: busca por ID inexistente).
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
