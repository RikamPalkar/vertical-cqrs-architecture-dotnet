using MediatR;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Features.Todos.DeleteTodo;

/// <summary>
/// HANDLER: Removes the todo from the database.
/// </summary>
public class DeleteTodoHandler : IRequestHandler<DeleteTodoCommand, bool>
{
    private readonly AppDbContext _db;

    public DeleteTodoHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _db.Todos.FindAsync([request.Id], cancellationToken);
        if (todo is null)
            return false;

        _db.Todos.Remove(todo);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
