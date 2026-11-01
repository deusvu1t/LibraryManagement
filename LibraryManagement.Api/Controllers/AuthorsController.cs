using LibraryManagement.Api.Entities;

using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private static List<Author> _authors =
    [
        new Author { Id = 1, FirstName = "Alexandre", LastName = "Dumas", YearOfBirth = 1802 },
        new Author { Id = 2, FirstName = "Agatha", LastName = "Christie", YearOfBirth = 1890 },
        new Author { Id = 3, FirstName = "Fyodor", LastName = "Dostoevsky", YearOfBirth = 1821 }
    ];

    [HttpGet]
    public async Task<ActionResult<List<Author>>> GetAll()
    {
        return Ok(_authors);
    }
}
