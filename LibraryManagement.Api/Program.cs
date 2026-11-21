using FluentValidation;

using LibraryManagement.Api.DataAccess;
using LibraryManagement.Api.DTOs.Categories;
using LibraryManagement.Api.DTOs.Members;
using LibraryManagement.Api.Endpoints;
using LibraryManagement.Api.Filters;
using LibraryManagement.Api.Mapping;
using LibraryManagement.Api.Middleware;
using LibraryManagement.Api.Services;
using LibraryManagement.Api.Services.Contracts;
using LibraryManagement.Api.Validators.Categories;
using LibraryManagement.Api.Validators.Members;

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
builder.Services.AddScoped<BookMapper>();
builder.Services.AddScoped<MemberMapper>();
ValidatorOptions.Global.LanguageManager.Enabled = false;
builder.Services.AddScoped<IValidator<CreateCategoryDto>, CreateCategoryDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateCategoryDto>, UpdateCategoryDtoValidator>();
builder.Services.AddScoped<IValidator<CreateMemberDto>, CreateMemberDtoValidator>();
builder.Services.AddScoped<IValidator<UpdateMemberDto>, UpdateMemberDtoValidator>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<LogActionFilter>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IBookService, BookService>();

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
app.MapMembersEndpoints();
app.MapControllers();

app.Run();
