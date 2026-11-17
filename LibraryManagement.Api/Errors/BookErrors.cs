using ErrorOr;

namespace LibraryManagement.Api.Errors;

public static class BookErrors
{
    public static Error BookNotFound(int id) =>
        Error.NotFound(
            description: $"A book with Id {id} has not been found.");

    public static Error CategoryNotFound(int id) =>
        Error.Validation(
            description: $"Category with id {id} was not found.");

    public static Error CategoryDoesNotExist(int id) =>
        Error.Validation(
            description: $"Category with Id {id} does not exist.");

    public static Error AuthorsNotFound() =>
        Error.Validation(
            description: "One or more of the provided author ids is not valid.");
}
