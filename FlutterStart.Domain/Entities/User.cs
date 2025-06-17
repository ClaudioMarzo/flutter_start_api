
using FlutterStart.Entities;

namespace FlutterStart.Domain.Entities;

public class User : BaseEntity
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Senha { get; set; }
    public string? Role { get; set; }
}

