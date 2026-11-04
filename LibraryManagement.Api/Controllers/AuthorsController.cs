using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController(LibraryDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadAuthorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadAuthorDto>>> GetAll()
    {
        var readDtos = await dbContext.Authors
            .Select(author => new ReadAuthorDto
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                YearOfBirth = author.YearOfBirth,
            }).ToListAsync();

        return Ok(readDtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDto>> GetById(int id)
    {
        var author = await dbContext.Authors.FindAsync(id);

        if (author is null)
        {
            return Problem(
                detail: $"An author with Id {id} has been not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        var readDto = new ReadAuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            YearOfBirth = author.YearOfBirth
        };

        return Ok(readDto);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadAuthorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadAuthorDto>> Create(CreateAuthorDto createDto)
    {
        if (string.IsNullOrWhiteSpace(createDto.FirstName) ||
            string.IsNullOrWhiteSpace(createDto.LastName))
        {
            return Problem(
                detail: "Author's First name and last name cannot be empty.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var author = new Author
        {
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            YearOfBirth = createDto.YearOfBirth
        };

        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync();

        var readDto = new ReadAuthorDto
        {
            Id = author.Id,
            FirstName = author.FirstName,
            LastName = author.LastName,
            YearOfBirth = author.YearOfBirth
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = author.Id },
            readDto);
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDto>> Update(int id, UpdateAuthorDto updateDto)
    {
        if (string.IsNullOrWhiteSpace(updateDto.FirstName) ||
            string.IsNullOrWhiteSpace(updateDto.LastName))
        {
            return Problem(
                detail: "Author's first name and last name cannot be empty.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var existingAuthor = await dbContext.Authors.FindAsync(id);

        if (existingAuthor is null)
        {
            return Problem(
                detail: $"An author with Id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        existingAuthor.FirstName = updateDto.FirstName;
        existingAuthor.LastName = updateDto.LastName;
        existingAuthor.YearOfBirth = updateDto.YearOfBirth;
        await dbContext.SaveChangesAsync();

        var readDto = new ReadAuthorDto
        {
            Id = existingAuthor.Id,
            FirstName = existingAuthor.FirstName,
            LastName = existingAuthor.LastName,
            YearOfBirth = existingAuthor.YearOfBirth
        };

        return Ok(readDto);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var authorToBeDeleted = await dbContext.Authors.FindAsync(id);

        if (authorToBeDeleted is null)
        {
            return Problem(
                detail: $"An author with Id {id} has been not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        dbContext.Authors.Remove(authorToBeDeleted);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
