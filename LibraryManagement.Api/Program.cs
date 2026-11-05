using FluentValidation;
using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Categories;
using LibraryManagement.Api.Endpoints;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.Validators.Categories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("LibraryDb")));
builder.Services.AddScoped<CategoryMapper>();
builder.Services.AddScoped<AuthorMapper>();
ValidatorOptions.Global.LanguageManager.Enabled = false;
builder.Services.AddScoped<IValidator<CreateCategoryDto>, CreateCategoryDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateCategoryDto>, UpdateCategoryDtoValidator>();

var app = builder.Build();

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    context.Response.OnStarting(() =>
    {
        stopwatch.Stop();
        context.Response.Headers["X-Response-Time-Ms"] = stopwatch.ElapsedMilliseconds.ToString();
        return Task.CompletedTask;
    });

    await next(context);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await DataSeeder.SeedAsync(app.Services);
}



app.MapCategoryEndpoints();
app.MapControllers();

app.Run();