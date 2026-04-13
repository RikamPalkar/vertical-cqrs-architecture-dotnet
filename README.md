
# Vertical Slice + CQRS Architecture in .NET

> A production-ready reference implementation of Vertical Slice Architecture
> and CQRS in .NET 8 — built feature-first, not layer-first.

[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![Language](https://img.shields.io/badge/Language-C%23-green)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-yellow)](LICENSE)

---

## Why This Exists

Most .NET tutorials teach you to build apps in horizontal layers:
`Controllers/` → `Services/` → `Repositories/` → `Models/`

It works for small apps. Then it doesn't.

When your codebase grows, a single feature — say, "Create Todo" — is
scattered across 4+ folders. Changing one thing means touching everything.

**Vertical Slice Architecture fixes this.**
Every feature is a self-contained slice. One folder. One responsibility.
Everything that belongs to "Create Todo" lives in `Features/Todos/CreateTodo/`.

This repo is the companion code for the two-part article series:

- 📖 [Part 1: Feature-First Design](https://www.c-sharpcorner.com/article/modern-backend-architecture-in-net-feature-first-design/)
- 📖 [Part 2: Implementing Vertical Slice + CQRS](https://www.c-sharpcorner.com/article/modern-backend-architecture-in-net-implementing-vertical-slice-cqrs/)

---

## What's Inside

A fully working **Todo API** built with:

- **Vertical Slice Architecture** — code organized by feature, not by layer
- **CQRS** — Commands (writes) and Queries (reads) are strictly separated
- **MediatR** — decouples endpoints from handlers via the mediator pattern
- **FluentValidation** — validation pipeline runs before any handler executes
- **EF Core InMemory** — no database setup needed, runs out of the box
- **Minimal APIs** — clean, lightweight endpoint registration
- **Global Exception Handling** — `ValidationException` → 400, others → 500

---

## Folder Structure

```
vertical-cqrs-architecture-dotnet/
│
├── Features/
│   └── Todos/                        # Everything for the Todos feature
│       ├── CreateTodo/               # Slice: Create a todo
│       │   ├── CreateTodoCommand.cs  # Command + Result DTO
│       │   ├── CreateTodoHandler.cs  # Business logic
│       │   └── CreateTodoValidator.cs# FluentValidation rules
│       ├── GetTodo/                  # Slice: Get one todo
│       ├── GetTodos/                 # Slice: Get all todos
│       ├── UpdateTodo/               # Slice: Update a todo
│       ├── DeleteTodo/               # Slice: Delete a todo
│       └── TodoEndpoints.cs          # All HTTP routes for Todos in one place
│
├── Infrastructure/
│   ├── Data/
│   │   └── AppDbContext.cs           # EF Core context (InMemory)
│   └── ValidationBehavior.cs         # MediatR pipeline: validates before handler runs
│
├── Core/
│   └── Todo.cs                       # Todo entity (shared domain model)
│
├── Program.cs                        # App entry point, service registration
└── TodoApi.csproj
```

---

## Architecture at a Glance

### Vertical Slice vs Traditional Layered

| Traditional Layered | Vertical Slice (This Project) |
|---|---|
| `Controllers/TodoController.cs` | `Features/Todos/TodoEndpoints.cs` |
| `Services/TodoService.cs` | `Features/Todos/CreateTodo/CreateTodoHandler.cs` |
| `Repositories/TodoRepo.cs` | Handler talks directly to `AppDbContext` |
| `Models/Todo.cs` | `Core/Todo.cs` (entity) + result DTOs per slice |

One feature. One folder. Zero cross-cutting clutter.

### Request Flow

```
HTTP Request
    ↓
TodoEndpoints.cs       (maps route, creates Command or Query)
    ↓
MediatR.Send()         (dispatches to the right handler)
    ↓
ValidationBehavior     (FluentValidation runs here — before handler)
    ↓
Handler                (business logic: read/write via AppDbContext)
    ↓
HTTP Response
```

### CQRS Split

| Type | Class | What it does |
|---|---|---|
| Command | `CreateTodoCommand` | Creates a new todo |
| Command | `UpdateTodoCommand` | Updates title, description, or status |
| Command | `DeleteTodoCommand` | Deletes a todo by ID |
| Query | `GetTodoQuery` | Returns a single todo |
| Query | `GetTodosQuery` | Returns all todos |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run the API

```bash
git clone https://github.com/RikamPalkar/vertical-cqrs-architecture-dotnet.git
cd vertical-cqrs-architecture-dotnet
dotnet run
```

The browser will redirect to Swagger at `http://localhost:{port}/swagger`.

### Try It Out

```http
POST /api/todos
{
  "title": "Learn Vertical Slice",
  "description": "Read both articles and explore the code"
}

GET /api/todos
GET /api/todos/{id}
PUT /api/todos/{id}
DELETE /api/todos/{id}
```

---

## Key Concepts Demonstrated

### 1. One Slice = One Use Case

```csharp
// Features/Todos/CreateTodo/CreateTodoCommand.cs
public record CreateTodoCommand(string Title, string? Description)
    : IRequest<CreateTodoResult>;

public record CreateTodoResult(int Id, string Title);
```

The command carries only what's needed. The result carries only what's returned.
No shared service. No bloated interface.

### 2. Handler Contains All the Logic

```csharp
public class CreateTodoHandler(AppDbContext db)
    : IRequestHandler<CreateTodoCommand, CreateTodoResult>
{
    public async Task<CreateTodoResult> Handle(
        CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = new Todo { Title = request.Title, Description = request.Description };
        db.Todos.Add(todo);
        await db.SaveChangesAsync(cancellationToken);
        return new CreateTodoResult(todo.Id, todo.Title);
    }
}
```

### 3. Validation Pipeline — Handler Never Sees Invalid Data

```csharp
// Infrastructure/ValidationBehavior.cs
// Runs all IValidator<TRequest> before the handler.
// Throws ValidationException on failure → global handler returns 400.
```

Validation rules live in the slice. Enforcement lives in the pipeline.
Handlers stay clean.

### 4. Feature Owns Its Routes

```csharp
// Features/Todos/TodoEndpoints.cs
public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        app.MapPost("/api/todos", async (CreateTodoRequest req, IMediator mediator) => ...);
        app.MapGet("/api/todos/{id}", async (int id, IMediator mediator) => ...);
        // ...
    }
}
```

No controller. No base class. The feature is fully self-contained.

---

## Tech Stack

| Technology | Purpose |
|---|---|
| .NET 8 | Runtime and framework |
| C# 12 | Language (records, primary constructors) |
| ASP.NET Core Minimal APIs | HTTP layer |
| MediatR | CQRS dispatch + pipeline behaviors |
| FluentValidation | Input validation |
| EF Core (InMemory) | Persistence (swap for SQL with one line) |
| Swagger / Swashbuckle | API documentation and testing |

---

## Swap to a Real Database

The InMemory database is for learning. To use SQL Server or PostgreSQL,
replace one line in `Program.cs`:

```csharp
// From:
options.UseInMemoryDatabase("TodoDb")

// To (SQL Server):
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
```

Then run `dotnet ef migrations add Init` and `dotnet ef database update`.

---

## When to Use This Pattern

✅ Medium to large APIs with many independent features  
✅ Teams where different developers own different features  
✅ Projects where features change independently  
✅ When you want to avoid bloated service classes  

❌ Simple CRUD with 2–3 endpoints (layered is fine)  
❌ Tiny scripts or utilities  

---

## Read the Full Series

This repo is the working code behind a two-part deep-dive:

- **[Part 1 — Feature-First Design](https://www.c-sharpcorner.com/article/modern-backend-architecture-in-net-feature-first-design/)**
  What Vertical Slice Architecture and CQRS are, why they complement each other,
  and when to use them. Includes diagrams and a comparison with layered architecture.

- **[Part 2 — Implementation](https://www.c-sharpcorner.com/article/modern-backend-architecture-in-net-implementing-vertical-slice-cqrs/)**
  How the Todo API is actually built — step by step, file by file.

---

## Contributing

Found an improvement? Want to add another feature slice?

1. Fork the repo
2. Create a feature branch (`git checkout -b feature/orders-slice`)
3. Commit your changes
4. Open a Pull Request

Ideas: add an Orders feature, plug in a real database, add integration tests,
implement a notification pipeline behavior.

---

## License

MIT License — use this however you want.

---

## Connect

**Rikam Palkar** — Senior Software Engineer, Microsoft MVP

- 🌐 [rikampalkar.github.io](https://rikampalkar.github.io)
- 💼 [LinkedIn](https://www.linkedin.com/in/rikampalkar/)
- ✍️ [Medium](https://medium.com/@RikamPalkar)
- 🐙 [GitHub](https://github.com/RikamPalkar)

---

*Stop organizing by layer. Start organizing by feature.*

---

This is ready to paste directly into your `README.md`. Want me to also generate the folder diagram as an actual visual, or create a `CONTRIBUTING.md` to go with it?
