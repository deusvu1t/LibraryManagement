using System.Data;

using LibraryManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.DataAccess;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        if (await dbContext.Authors.AnyAsync() ||
            await dbContext.Categories.AnyAsync() ||
            await dbContext.Books.AnyAsync() ||
            await dbContext.Members.AnyAsync())
        {
            return;
        }

        var seedAuthors = new List<Author>
        {
            new() { FirstName = "George", LastName = "Orwell", YearOfBirth = 1903 },
            new() { FirstName = "Jane", LastName = "Austen", YearOfBirth = 1775 },
            new() { FirstName = "Mark", LastName = "Twain", YearOfBirth = 1835 },
            new() { FirstName = "J.K.", LastName = "Rowling", YearOfBirth = 1965 },
            new() { FirstName = "Ernest", LastName = "Hemingway", YearOfBirth = 1899 },
            new() { FirstName = "Neil", LastName = "Gaiman", YearOfBirth = 1960 },
            new() { FirstName = "Terry", LastName = "Pratchett", YearOfBirth = 1948 },
            new() { FirstName = "Mary", LastName = "Shelley", YearOfBirth = 1797 },
            new() { FirstName = "J.R.R.", LastName = "Tolkien", YearOfBirth = 1892 },
            new() { FirstName = "Octavia", LastName = "Butler", YearOfBirth = 1947 }
        };

        var authors = seedAuthors.ToDictionary(author => author.LastName);
        string[] categoryNames = ["Fiction", "Non-Fiction", "Science Fiction", "Fantasy", "Biography"];
        var categories = categoryNames
            .Select(name => new Category { Name = name })
            .ToDictionary(category => category.Name);

        Book CreateBook(string title, string categoryName, int totalCopies, params string[] authorLastNames)
        {
            var category = categories[categoryName];

            return new Book
            {
                Title = title,
                TotalCopies = totalCopies,
                AvailableCopies = totalCopies,
                CategoryId = category.Id,
                Category = category,
                Authors = authorLastNames.Select(lastName => authors[lastName]).ToList()
            };
        }

        var books = new List<Book>
        {
            CreateBook("1984", "Science Fiction", 5, "Orwell"),
            CreateBook("Animal Farm", "Fiction", 4, "Orwell"),
            CreateBook("Pride and Prejudice", "Fiction", 3, "Austen"),
            CreateBook("Adventures of Huckleberry Finn", "Fiction", 4, "Twain"),
            CreateBook("Harry Potter and the Philosopher's Stone", "Fantasy", 6, "Rowling"),
            CreateBook("The Old Man and the Sea", "Fiction", 3, "Hemingway"),
            CreateBook("Good Omens", "Fantasy", 5, "Gaiman", "Pratchett"),
            CreateBook("American Gods", "Fantasy", 3, "Gaiman"),
            CreateBook("The Colour of Magic", "Fantasy", 3, "Pratchett"),
            CreateBook("Frankenstein", "Science Fiction", 4, "Shelley")
        };

        var members = new List<Member>
        {
            new() { FirstName = "Alice", LastName = "Morgan" },
            new() { FirstName = "Benjamin", LastName = "Reed" },
            new() { FirstName = "Clara", LastName = "Bennett" },
            new() { FirstName = "Daniel", LastName = "Foster" },
            new() { FirstName = "Emma", LastName = "Hayes" },
            new() { FirstName = "Felix", LastName = "Turner" },
            new() { FirstName = "Grace", LastName = "Parker" },
            new() { FirstName = "Henry", LastName = "Brooks" },
            new() { FirstName = "Isabel", LastName = "Collins" },
            new() { FirstName = "Jack", LastName = "Mitchell" }
        };

        dbContext.Authors.AddRange(seedAuthors);
        dbContext.Categories.AddRange(categories.Values);
        dbContext.Books.AddRange(books);
        dbContext.Members.AddRange(members);
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
