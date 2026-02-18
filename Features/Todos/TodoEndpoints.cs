using MediatR;
using TodoApi.Features.Todos.CreateTodo;
using TodoApi.Features.Todos.DeleteTodo;
using TodoApi.Features.Todos.GetTodo;
using TodoApi.Features.Todos.GetTodos;
using TodoApi.Features.Todos.UpdateTodo;

namespace TodoApi.Features.Todos;

/// <summary>
/// ENDPOINT MAPPING: All Todo-related Minimal API routes live here.
/// Each route receives the request, sends a Command/Query via MediatR, returns the result.
/// This keeps the HTTP layer thin - no business logic, just "send to MediatR".
/// </summary>
public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos").WithTags("Todos");

        // GET /api/todos -> Get all todos (optional ?isCompleted=true|false)
        group.MapGet("/", async (bool? isCompleted, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTodosQuery(isCompleted));
            return Results.Ok(result);
        });

        // GET /api/todos/{id} -> Get one todo
        group.MapGet("/{id:int}", async (int id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTodoQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        // POST /api/todos -> Create a todo
        group.MapPost("/", async (CreateTodoRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateTodoCommand(request.Title, request.Description));
            return Results.Created($"/api/todos/{result.Id}", result);
        });

        // PUT /api/todos/{id} -> Update a todo
        group.MapPut("/{id:int}", async (int id, UpdateTodoRequest request, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateTodoCommand(
                id, request.Title, request.Description, request.IsCompleted));
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        // DELETE /api/todos/{id}
        group.MapDelete("/{id:int}", async (int id, IMediator mediator) =>
        {
            var deleted = await mediator.Send(new DeleteTodoCommand(id));
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    // Request DTOs for POST/PUT (what the client sends in the body)
    public record CreateTodoRequest(string Title, string? Description);
    public record UpdateTodoRequest(string? Title, string? Description, bool? IsCompleted);
}
