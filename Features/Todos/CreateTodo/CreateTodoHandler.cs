using MediatR;
using TodoApi.Core;
using TodoApi.Infrastructure.Data;

namespace TodoApi.Features.Todos.CreateTodo;

/// <summary>
/// HANDLER: Contains the business logic for CreateTodoCommand.
/// MediatR will find this handler when you send a CreateTodoCommand.
/// One command → one handler (easy to locate and test).
/// </summary>
public class CreateTodoHandler : IRequestHandler<CreateTodoCommand, CreateTodoResult>
{
    private readonly AppDbContext _db;

    public CreateTodoHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CreateTodoResult> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = new Todo
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false
        };

        _db.Todos.Add(todo);
        await _db.SaveChangesAsync(cancellationToken);

        return new CreateTodoResult(todo.Id, todo.Title);
    }
}
