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
    [ProducesResponseType(typeof(List<Author>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Author>>> GetAll()
    {
        return Ok(_authors);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Author), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Author>> GetById(int id)
    {
        var author = _authors.FirstOrDefault(
            author => author.Id == id);

        if (author is null)
        {
            return Problem(
                detail: $"An author with Id {id} has been not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(author);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Author), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Author>> Create(Author author)
    {
        if (string.IsNullOrWhiteSpace(author.FirstName) ||
            string.IsNullOrWhiteSpace(author.LastName))
        {
            return Problem(
                detail: "Author's First name and last name cannot be empty.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (_authors.Any(existingAuthor => existingAuthor.Id == author.Id))
        {
            return Problem(
                detail: $"An author with Id {author.Id} already exists.",
                statusCode: StatusCodes.Status409Conflict);
        }

        _authors.Add(author);

        return CreatedAtAction(
            nameof(GetById),
            new { id = author.Id },
            author);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Author), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Author>> Update(int id, Author author)
    {
        if (string.IsNullOrWhiteSpace(author.FirstName) ||
            string.IsNullOrWhiteSpace(author.LastName))
        {
            return Problem(
                detail: "Author's first name and last name cannot be empty.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (id != author.Id)
        {
            return Problem(
                detail: "Author Id in the URL does not match the request body.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var existingAuthor = _authors.FirstOrDefault(
            existingAuthor => existingAuthor.Id == id);

        if (existingAuthor is null)
        {
            return Problem(
                detail: $"An author with Id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        existingAuthor.FirstName = author.FirstName;
        existingAuthor.LastName = author.LastName;
        existingAuthor.YearOfBirth = author.YearOfBirth;

        return Ok(existingAuthor);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var authorToBeDeleted = _authors.FirstOrDefault(
            author => author.Id == id);

        if (authorToBeDeleted is null)
        {
            return Problem(
                detail: $"An author with Id {id} has been not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        _authors.Remove(authorToBeDeleted);

        return NoContent();
    }
}
