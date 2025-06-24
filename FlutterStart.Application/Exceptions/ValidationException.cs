namespace FlutterStart.Application.Exceptions
{
    // Exceção lançada quando há erro de validação de dados fornecidos pelo cliente.
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}
