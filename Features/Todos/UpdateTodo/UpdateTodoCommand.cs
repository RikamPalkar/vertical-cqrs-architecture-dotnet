using MediatR;

namespace TodoApi.Features.Todos.UpdateTodo;

/// <summary>
/// COMMAND: Update an existing todo (title, description, or completion status).
/// </summary>
public record UpdateTodoCommand(
    int Id,
    string? Title,
    string? Description,
    bool? IsCompleted
) : IRequest<UpdateTodoResult?>;

public record UpdateTodoResult(int Id, string Title, bool IsCompleted);
