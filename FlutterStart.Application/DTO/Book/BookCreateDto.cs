using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FlutterStart.Application.DTO.Book;

public class BookCreateDto
{
    [Required (ErrorMessage = "O título do livro é obrigatório")]
    [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres.")]
    public string? Title { get; set; }
    
    [Required (ErrorMessage = "O resumo do livro é obrigatório")]
    [StringLength(500, ErrorMessage = "O resumo deve ter no máximo 500 caracteres.")]
    public string? Summary { get; set; }

    [Required (ErrorMessage = "O autor do produto é obrigatório")]
    [StringLength(50, ErrorMessage = "O autor deve ter no máximo 50 caracteres.")]
    public string? Author { get; set; }

    [Required (ErrorMessage = "O ISBN do produto é obrigatório")]
    [StringLength(20, ErrorMessage = "O ISBN deve ter no máximo 20 caracteres.")]
    public string? Isbn { get; set; }

    [Required (ErrorMessage = "O ano de publicação do produto é obrigatório")]
    [Range(1000, 9999, ErrorMessage = "O ano de publicação deve estar entre 1000 e 9999.")]
    public int PublicationYear { get; set; }

    [Required (ErrorMessage = "O número de páginas do produto é obrigatório")]
    [Range(1, 10000, ErrorMessage = "O número de páginas deve estar entre 1 e 10000.")]
    public int PageCount { get; set; }

    [Required (ErrorMessage = "A editora do produto é obrigatória")]
    [StringLength(50, ErrorMessage = "A editora deve ter no máximo 50 caracteres.")]
    public string? Publisher { get; set; }

    [Required(ErrorMessage = "A edição do produto é obrigatória")]
    [StringLength(50, ErrorMessage = "A edição deve ter no máximo 50 caracteres.")]
    public string? Edition { get; set; }

    [Required(ErrorMessage = "O gênero do produto é obrigatório")]
    [StringLength(20, ErrorMessage = "O gênero deve ter no máximo 20 caracteres.")]
    public string? Genre { get; set; }

    [Required(ErrorMessage = "A imagem do produto é obrigatória")]
    public IFormFile? ImageUrl { get; set; }

    [Required(ErrorMessage = "O idioma do livro é obrigatório")]
    [StringLength(20, ErrorMessage = "O idioma deve ter no máximo 20 caracteres.")]
    public string? Language { get; set; }

    [Required(ErrorMessage = "O formato do livro é obrigatório")]
    [StringLength(20, ErrorMessage = "O formato deve ter no máximo 20 caracteres.")]
    public string? Format { get; set; }

    [Required(ErrorMessage = "As dimensões do livro são obrigatórias")]
    [StringLength(30, ErrorMessage = "As dimensões devem ter no máximo 30 caracteres.")]
    public string? Dimensions { get; set; }

    [Required(ErrorMessage = "A localização do livro é obrigatória")]
    [StringLength(50, ErrorMessage = "A localização deve ter no máximo 50 caracteres.")]
    public string? Location { get; set; }
}