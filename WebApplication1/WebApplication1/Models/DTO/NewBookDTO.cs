namespace WebApplication1.Controllers;

public class NewBookDTO
{
    public string Title { get; set; }
    public HashSet<int> GenreIds { get; set; } 
}