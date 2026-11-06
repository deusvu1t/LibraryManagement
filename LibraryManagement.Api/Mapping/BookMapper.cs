using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Entities;

using Riok.Mapperly.Abstractions;

namespace LibraryManagement.Api.Mapping;

[Mapper]
public partial class BookMapper
{
    public partial ReadBookDto ToReadDto(Book source);

    [MapperIgnoreTarget(nameof(Book.Id))]
    [MapperIgnoreTarget(nameof(Book.AvailableCopies))]
    public partial Book ToEntity(CreateBookDto source);

    [MapperIgnoreTarget(nameof(Book.Id))]
    [MapperIgnoreTarget(nameof(Book.AvailableCopies))]
    public partial void UpdateEntity(UpdateBookDto source, Book target);
}
