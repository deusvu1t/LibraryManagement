using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Mapping;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(
    LibraryDbContext dbContext,
    BookMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ReadBookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReadBookDto>>> GetAll()
    {
        var books = await dbContext.Books
        .Include(book => book.Category)
        .Include(book => book.Authors)
        .ToListAsync();

        var readDtos = books
            .Select(mapper.ToReadDto)
            .ToList();

        return Ok(readDtos);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBookDto>> GetById(int id)
    {
        var book = await dbContext.Books
            .Include(book => book.Category)
            .Include(book => book.Authors)
            .FirstOrDefaultAsync(book => book.Id == id);

        if (book is null)
        {
            return Problem(
                detail: $"A book with Id {id} has been not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(mapper.ToReadDto(book));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReadBookDto>> Create(CreateBookDto createDto)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(
            category => category.Id == createDto.CategoryId);

        if (category is null)
        {
            return Problem(
                detail: $"Category with Id {createDto.CategoryId} does not exist.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var authors = await dbContext.Authors
            .Where(author => createDto.AuthorIds.Contains(author.Id))
            .ToListAsync();

        if (authors.Count != createDto.AuthorIds.Count)
        {
            return Problem(
                detail: "One or more of the provided author ids is not valid.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var book = mapper.ToEntity(createDto);
        book.AvailableCopies = book.TotalCopies;
        book.Category = category;
        book.Authors = authors;

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = book.Id },
            mapper.ToReadDto(book));
    }


    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ReadBookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBookDto>> Update(int id, UpdateBookDto updateDto)
    {
        var existingBook = await dbContext.Books.FindAsync(id);

        if (existingBook is null)
        {
            return Problem(
                detail: $"A book with Id {id} has not been found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        mapper.UpdateEntity(updateDto, existingBook);
        await dbContext.SaveChangesAsync();

        return Ok(mapper.ToReadDto(existingBook));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var bookToBeDeleted = await dbContext.Books.FindAsync(id);

        if (bookToBeDeleted is null)
        {
            return Problem(
                detail: $"A book with Id {id} has been not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        dbContext.Books.Remove(bookToBeDeleted);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
