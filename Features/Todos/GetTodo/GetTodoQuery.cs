using MediatR;

namespace TodoApi.Features.Todos.GetTodo;

/// <summary>
/// QUERY: Represents a read request - "get one todo by id".
/// Queries do not change state; they only return data.
/// </summary>
public record GetTodoQuery(int Id) : IRequest<GetTodoResult?>;

/// <summary>
/// DTO returned when we fetch a single todo.
/// </summary>
public record GetTodoResult(int Id, string Title, string? Description, bool IsCompleted, DateTime CreatedAt);
