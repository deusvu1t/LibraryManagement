namespace LibraryManagement.Api.DTOs.Books;

public class ReadBookDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required int AvailableCopies { get; init; }
    public required int TotalCopies { get; init; }
    public string CategoryName { get; init; } = null!;
}
