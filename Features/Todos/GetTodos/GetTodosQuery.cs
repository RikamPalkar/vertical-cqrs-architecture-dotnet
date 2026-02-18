using MediatR;

namespace TodoApi.Features.Todos.GetTodos;

/// <summary>
/// QUERY: Get all todos (optionally filter by completed status).
/// </summary>
public record GetTodosQuery(bool? IsCompleted = null) : IRequest<GetTodosResult>;

public record GetTodosResult(IReadOnlyList<GetTodosItem> Items);

public record GetTodosItem(int Id, string Title, string? Description, bool IsCompleted, DateTime CreatedAt);
