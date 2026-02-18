using MediatR;

namespace TodoApi.Features.Todos.CreateTodo;

/// <summary>
/// COMMAND: Represents the intent to create a new Todo.
/// In CQRS, commands change state and typically return minimal data (e.g. the new ID).
/// They implement IRequest{TResponse}.
/// </summary>
public record CreateTodoCommand(string Title, string? Description) : IRequest<CreateTodoResult>;

/// <summary>
/// What we return after creating a todo (e.g. the new id and resource URL).
/// </summary>
public record CreateTodoResult(int Id, string Title);
