using ErrorOr;

using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Errors;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.Services.Contracts;

using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Services;

public class BookService(
    LibraryDbContext dbContext,
    BookMapper mapper) : IBookService
{
    public async Task<ErrorOr<ReadBookDto>> CreateAsync(CreateBookDto createDto)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(
           category => category.Id == createDto.CategoryId);

        if (category is null)
        {
            return BookErrors.CategoryNotFound(createDto.CategoryId);
        }

        var uniqueAuthorIds = createDto.AuthorIds.Distinct().ToList();

        var authors = await dbContext.Authors
            .Where(author => uniqueAuthorIds.Contains(author.Id))
            .ToListAsync();

        if (authors.Count != uniqueAuthorIds.Count)
        {
            return BookErrors.AuthorsNotFound();
        }

        var book = mapper.ToEntity(createDto);
        book.AvailableCopies = book.TotalCopies;
        book.Category = category;
        book.Authors = authors;

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();

        return mapper.ToReadDto(book);
    }
}
