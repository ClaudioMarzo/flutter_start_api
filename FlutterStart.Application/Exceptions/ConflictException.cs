
namespace FlutterStart.Application.Exceptions
{
    // Exceção lançada quando há conflito de dados (ex: registro duplicado).
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
