using System.ComponentModel.DataAnnotations;

namespace FlutterStart.Application.DTO;

public class LoanRequestDto
{
    [Required(ErrorMessage = "O ID do livro é obrigatório")]
    public int BookId { get; set; }

    [Required(ErrorMessage = "O ID do usuário é obrigatório")]
    public int UserId { get; set; }
    public string? Observations { get; set; }
}