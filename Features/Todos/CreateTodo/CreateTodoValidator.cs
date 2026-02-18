using FluentValidation;

namespace TodoApi.Features.Todos.CreateTodo;

/// <summary>
/// VALIDATOR: Ensures the command has valid data before the handler runs.
/// Optional but recommended for commands that accept user input.
/// </summary>
public class CreateTodoValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters");
    }
}
