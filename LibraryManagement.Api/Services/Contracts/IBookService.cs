using LibraryManagement.Api.DTOs.Books;

namespace LibraryManagement.Api.Services.Contracts;

public interface IBookService
{
    Task<ReadBookDto> CreateAsync(CreateBookDto createDto);
}
