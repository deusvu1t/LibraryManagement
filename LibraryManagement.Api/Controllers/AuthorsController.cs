using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Authors;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.Mapping;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[ServiceFilter(typeof(LogActionFilter))]
[Route("api/[controller]")]
public class AuthorsController(
    LibraryDbContext dbContext,
    AuthorMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadAuthorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadAuthorDto>>> GetAll()
    {
        var authors = await dbContext.Authors.ToListAsync();

        var readDtos = authors
            .Select(mapper.ToReadDto)
            .ToList();

        return Ok(readDtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDetailDto>> GetById(int id)
    {
        var author = await dbContext.Authors
        .Include(author => author.Books)
        .FirstOrDefaultAsync(author => author.Id == id);

        if (author is null)
        {
            return Problem(
                detail: $"An author with Id {id} has been not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(mapper.ToReadDetailDto(author));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadAuthorDetailDto>> Create(CreateAuthorDto createDto)
    {
        var author = mapper.ToEntity(createDto);

        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = author.Id },
            mapper.ToReadDetailDto(author));
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadAuthorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadAuthorDetailDto>> Update(int id, UpdateAuthorDto updateDto)
    {
        var existingAuthor = await dbContext.Authors
        .Include(author => author.Books)
        .FirstOrDefaultAsync(author => author.Id == id);

        if (existingAuthor is null)
        {
            return Problem(
                detail: $"An author with Id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        mapper.UpdateEntity(updateDto, existingAuthor);
        await dbContext.SaveChangesAsync();

        return Ok(mapper.ToReadDetailDto(existingAuthor));
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
