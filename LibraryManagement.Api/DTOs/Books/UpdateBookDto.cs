using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTOs.Books;

public class UpdateBookDto
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [Required]
    [Range(0, int.MaxValue)]
    public required int TotalCopies { get; init; }
}
