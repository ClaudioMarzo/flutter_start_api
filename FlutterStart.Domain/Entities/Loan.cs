using FlutterStart.Entities;

namespace FlutterStart.Domain.Entities;

public class Loan : BaseEntity
{
    public virtual User? User { get; set; }
    public int UserId { get; set; }
    public virtual Book? Book { get; set; }
    public int BookId { get; set; }
    public bool IsReturned { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
}