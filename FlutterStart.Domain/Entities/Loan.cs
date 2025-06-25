using FlutterStart.Entities;

namespace FlutterStart.Domain.Entities;

public class Loan : BaseEntity
{
    public virtual User? User { get; set; }
    public int UserId { get; set; }
    public virtual Book? Book { get; set; }
    public int BookId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Status { get; set; }
    public string? Observations { get; set; }

    public static class LoanStatus
    {
        public const string Borrowed = "EMPRESTADO";
        public const string Returned = "DEVOLVIDO";
        public const string Overdue = "ATRASADO";
    }
}