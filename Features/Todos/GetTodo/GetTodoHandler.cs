using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Features.Todos.GetTodo;

/// <summary>
/// HANDLER: Fetches a single todo by id. Returns null if not found.
/// </summary>
public class GetTodoHandler : IRequestHandler<GetTodoQuery, GetTodoResult?>
{
    private readonly AppDbContext _db;

    public GetTodoHandler(AppDbContext db) => _db = db;

    public async Task<GetTodoResult?> Handle(GetTodoQuery request, CancellationToken cancellationToken)
    {
        var todo = await _db.Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (todo is null)
            return null;

        return new GetTodoResult(
            todo.Id,
            todo.Title,
            todo.Description,
            todo.IsCompleted,
            todo.CreatedAt
        );
    }
}
