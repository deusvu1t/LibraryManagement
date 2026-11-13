namespace LibraryManagement.Api.RequestParameters;

public record BookFilteringParameters(
    string? Title,
    int? CategoryId,
    int? AuthorId
);
