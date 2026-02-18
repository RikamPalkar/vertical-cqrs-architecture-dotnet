using MediatR;
using Microsoft.EntityFrameworkCore;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Features.Todos.GetTodos;

public class GetTodosHandler : IRequestHandler<GetTodosQuery, GetTodosResult>
{
    private readonly AppDbContext _db;

    public GetTodosHandler(AppDbContext db) => _db = db;

    public async Task<GetTodosResult> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Todos.AsNoTracking();

        if (request.IsCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == request.IsCompleted.Value);

        var items = await query
            .OrderBy(t => t.CreatedAt)
            .Select(t => new GetTodosItem(t.Id, t.Title, t.Description, t.IsCompleted, t.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetTodosResult(items);
    }
}
