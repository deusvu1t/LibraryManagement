using FluentValidation;

using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Categories;
using LibraryManagement.Api.Endpoints;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.Middleware;
using LibraryManagement.Api.Validators.Categories;

using Microsoft.EntityFrameworkCore;

using Scalar.AspNetCore;
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
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseMiddleware<RequestTimingMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    await DataSeeder.SeedAsync(app.Services);
}



app.MapCategoryEndpoints();
app.MapControllers();

app.Run();
