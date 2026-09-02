using EntityFramework.Exceptions.SqlServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using realworld_net.Data;
using realworld_net.Middleware;
using realworld_net.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// DB stuff
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseSqlServer(connectionString)
        .UseExceptionProcessor());

// Add services to the container.
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJWTService, JWTService>();

builder.Services.AddControllers();

// Custom exception handling middleware
builder.Services.AddExceptionHandler<DuplicateExceptionHandler>();
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Custom behaviour for validation errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        Dictionary<string, string[]> errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var errorResponse = new
        {
            Errors = errors
        };

        return new UnprocessableEntityObjectResult(errorResponse);
    };
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
