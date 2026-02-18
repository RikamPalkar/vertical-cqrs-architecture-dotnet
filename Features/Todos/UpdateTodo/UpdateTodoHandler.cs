using MediatR;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Features.Todos.UpdateTodo;

/// <summary>
/// HANDLER: Loads the todo, applies changes, saves. Returns null if todo not found.
/// </summary>
public class UpdateTodoHandler : IRequestHandler<UpdateTodoCommand, UpdateTodoResult?>
{
    private readonly AppDbContext _db;

    public UpdateTodoHandler(AppDbContext db) => _db = db;

    public async Task<UpdateTodoResult?> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _db.Todos.FindAsync([request.Id], cancellationToken);
        if (todo is null)
            return null;

        if (request.Title is { } title)
            todo.Title = title;
        if (request.Description is { } desc)
            todo.Description = desc;
        if (request.IsCompleted.HasValue)
            todo.IsCompleted = request.IsCompleted.Value;

        await _db.SaveChangesAsync(cancellationToken);

        return new UpdateTodoResult(todo.Id, todo.Title, todo.IsCompleted);
    }
}
