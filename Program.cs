using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TodoApi.Features.Todos;
using TodoApi.Features.Todos.CreateTodo;
using TodoApi.Infrastructure;
using TodoApi.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ----- Add services -----

// DbContext with InMemory (no real database needed)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TodoDb"));

// MediatR: scans the assembly and registers all IRequestHandler<,> and validators
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// FluentValidation: register all validators from this assembly
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Register the validation pipeline behavior so validators run before handlers
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Swagger for easy API testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Todo API (Vertical Slice + CQRS)", Version = "v1" });
});

var app = builder.Build();

// ----- Middleware -----

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handler: ValidationException -> 400 with messages; others -> 500
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        if (ex is ValidationException validationEx)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            var errors = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            await context.Response.WriteAsJsonAsync(new { errors });
            return;
        }
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = "An error occurred." });
    });
});

// Map all Todo endpoints (Vertical Slice: feature owns its routes)
app.MapTodoEndpoints();

// Health check for learning/demo
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
