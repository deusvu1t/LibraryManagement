using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.Services.Contracts;

namespace LibraryManagement.Api.Services;

public class BookService(
    LibraryDbContext dbContext,
    BookMapper mapper) : IBookService
{

    public async Task<ReadBookDto> CreateAsync(CreateBookDto createDto)
    {
        throw new NotImplementedException();
    }
}
