using MediatR;

namespace TodoApi.Features.Todos.DeleteTodo;

/// <summary>
/// COMMAND: Delete a todo by id. Returns true if deleted, false if not found.
/// </summary>
public record DeleteTodoCommand(int Id) : IRequest<bool>;
