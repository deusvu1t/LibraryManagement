using LibraryManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.DataAccess;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        if (!await dbContext.Authors.AnyAsync())
        {
            var authors = new List<Author>
            {
                new() { FirstName = "George", LastName = "Orwell", YearOfBirth = 1903 },
                new() { FirstName = "Jane", LastName = "Austen", YearOfBirth = 1775 },
                new() { FirstName = "Mark", LastName = "Twain", YearOfBirth = 1835 },
                new() { FirstName = "J.K.", LastName = "Rowling", YearOfBirth = 1965 },
                new() { FirstName = "Ernest", LastName = "Hemingway", YearOfBirth = 1899 }
            };

            await dbContext.Authors.AddRangeAsync(authors);
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.Categories.AnyAsync())
        {
            var categories = new List<Category>
            {
                new() { Name = "Fiction" },
                new() { Name = "Non-Fiction" },
                new() { Name = "Science Fiction" },
                new() { Name = "Fantasy" },
                new() { Name = "Biography" }
            };

            await dbContext.Categories.AddRangeAsync(categories);
            await dbContext.SaveChangesAsync();
        }
    }
}
