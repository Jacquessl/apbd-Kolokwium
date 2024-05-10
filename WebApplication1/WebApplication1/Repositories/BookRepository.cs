using Microsoft.Data.SqlClient;
using WebApplication1.Controllers;
using WebApplication1.Models.DTO;

namespace WebApplication1.Repositories;

public class BookRepository : IBookRepository
{
    private readonly IConfiguration _configuration;
    private IBookRepository _bookRepositoryImplementation;

    public BookRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<BookDTO> GetBookWithGenres(int id)
    {
        var query = "SELECT books.PK as book, title as tit, genres.name as genre FROM books " +
                    "JOIN books_genres BG ON BG.FK_BOOK = books.PK " +
                    "JOIN genres ON genres.PK = BG.FK_genre " +
                    "WHERE books.PK = @id";
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        var bookIdOrdidinal = reader.GetOrdinal("book");
        var bookTitleOridinal = reader.GetOrdinal("tit");
        var gernreName = reader.GetOrdinal("genre");

        BookDTO bookDTO = null; 
        while (await reader.ReadAsync())
        {
            if (bookDTO is not null)
            {
                bookDTO.Genres.Add(reader.GetString(gernreName));
            }
            else
            {
                bookDTO = new BookDTO()
                {
                    Id = reader.GetInt32(bookIdOrdidinal),
                    Title = reader.GetString(bookTitleOridinal),
                    Genres = new List<string>()
                    {
                        reader.GetString(gernreName)
                    }
                };
            }
        }

        if (bookDTO is null)
        {
            throw new Exception();
        }

        return bookDTO;
    }

    public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT 1 FROM Books WHERE PK = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;    
    }

    public async Task<bool> DoGenresExist(NewBookDTO newBookDto)
    {
        foreach (var id in newBookDto.GenreIds)
        {
            var query = "SELECT 1 FROM genres WHERE PK = @ID";

            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();

            command.Connection = connection;
            command.CommandText = query;
            command.Parameters.AddWithValue("@ID", id);

            await connection.OpenAsync();

            var res = await command.ExecuteScalarAsync();
            
            if (res is null)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<object> AddNewBook(NewBookDTO newBookDto)
    {
        var insert = @"INSERT INTO books VALUES(@title);
					   SELECT @@IDENTITY AS ID;";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
	    
        command.Connection = connection;
        command.CommandText = insert;
        command.Parameters.AddWithValue("@title", newBookDto.Title);
        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
	    
        try
        {
            var id = await command.ExecuteScalarAsync();

            foreach (var genreId in newBookDto.GenreIds)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO books_genres VALUES(@bookId, @genreId)";
                command.Parameters.AddWithValue("@bookId", id);
                command.Parameters.AddWithValue("@genreId", genreId);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            
            return id;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
        
    }
}