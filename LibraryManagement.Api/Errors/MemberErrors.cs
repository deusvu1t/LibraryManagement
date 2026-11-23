using ErrorOr;

namespace LibraryManagement.Api.Errors;

public static class MemberErrors
{
    public static Error MemberNotFound(int id) =>
        Error.NotFound(
            description: $"A member with Id {id} has not been found.");
}
