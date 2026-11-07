using LibraryManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.DataAccess;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        var existingAuthors = await dbContext.Authors.ToListAsync();
        var authors = new Dictionary<string, Author>();
        var seedAuthors = new List<Author>
        {
            new() { FirstName = "George", LastName = "Orwell", YearOfBirth = 1903 },
            new() { FirstName = "Jane", LastName = "Austen", YearOfBirth = 1775 },
            new() { FirstName = "Mark", LastName = "Twain", YearOfBirth = 1835 },
            new() { FirstName = "J.K.", LastName = "Rowling", YearOfBirth = 1965 },
            new() { FirstName = "Ernest", LastName = "Hemingway", YearOfBirth = 1899 },
            new() { FirstName = "Neil", LastName = "Gaiman", YearOfBirth = 1960 },
            new() { FirstName = "Terry", LastName = "Pratchett", YearOfBirth = 1948 }
        };

        foreach (var author in seedAuthors)
        {
            var existingAuthor = existingAuthors.FirstOrDefault(a =>
                a.FirstName == author.FirstName && a.LastName == author.LastName);

            if (existingAuthor is null)
            {
                dbContext.Authors.Add(author);
            }

            authors.Add(author.LastName, existingAuthor ?? author);
        }

        var existingCategories = await dbContext.Categories.ToListAsync();
        var categories = new Dictionary<string, Category>();
        string[] categoryNames = ["Fiction", "Non-Fiction", "Science Fiction", "Fantasy", "Biography"];

        foreach (var name in categoryNames)
        {
            var category = existingCategories.FirstOrDefault(c => c.Name == name);

            if (category is null)
            {
                category = new Category { Name = name };
                dbContext.Categories.Add(category);
            }

            categories.Add(name, category);
        }

        await dbContext.SaveChangesAsync();

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
            CreateBook("The Colour of Magic", "Fantasy", 3, "Pratchett")
        };

        var existingTitles = (await dbContext.Books.Select(b => b.Title).ToListAsync()).ToHashSet();

        foreach (var book in books)
        {
            if (existingTitles.Add(book.Title))
            {
                dbContext.Books.Add(book);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}
