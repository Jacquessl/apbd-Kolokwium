using WebApplication1.Controllers;
using WebApplication1.Models.DTO;

namespace WebApplication1.Repositories;

public interface IBookRepository
{
    Task<BookDTO> GetBookWithGenres(int id);
    Task<bool> DoesBookExist(int id);
    Task<bool> DoGenresExist(NewBookDTO newBookDto);
    Task<object> AddNewBook(NewBookDTO newBookDto);
}