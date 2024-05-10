using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.DTO;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookRepository _bookRepository;

    public BooksController(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    [HttpGet("{id}/genres")]
    public async Task<IActionResult> GetBooks(int id)
    {
        if (!await _bookRepository.DoesBookExist(id))
        {
            return NotFound($"Book with given ID - {id} doesn't exist");
        }
        var genres = await _bookRepository.GetBookWithGenres(id);

        return Ok(genres);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook([FromBody] NewBookDTO newBookDto)
    {
        if (!await _bookRepository.DoGenresExist(newBookDto))
        {
            return NotFound("One of given genres doesn't exist");
        }

        var id = await _bookRepository.AddNewBook(newBookDto);
        var bookDto = await _bookRepository.GetBookWithGenres(Convert.ToInt32(id));
        
        return Created(Request.Path.Value ?? "api/books", bookDto);
    }
}