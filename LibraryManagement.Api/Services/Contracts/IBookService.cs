using ErrorOr;

using LibraryManagement.Api.DTOs.Books;

namespace LibraryManagement.Api.Services.Contracts;

public interface IBookService
{
    Task<ErrorOr<ReadBookDto>> CreateAsync(CreateBookDto createDto);
}
