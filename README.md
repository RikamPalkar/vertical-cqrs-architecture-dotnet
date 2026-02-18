# vertical-cqrs-architecture-dotnet

# Vertical Slice Architecture + CQRS — A Beginner's Guide

This document explains the concepts behind this Todo API project and how they work together. It’s written for developers new to these patterns.

---

## This Is a 2-Article Series

| Article | What you’ll learn |
|--------|--------------------|
| **Article 1 (this one)** | The **big picture**: what Vertical Slice Architecture and CQRS are, why they work well together, when to use them (and when not to), a simple diagram, high-level request flow, real-world use cases, pros and cons, and how this approach compares to layered architecture. No project code yet — concepts only. |
| **Article 2** | The **implementation**: how this Todo API is built. You’ll see the actual project structure, how Commands and Queries are implemented with MediatR, how validators (FluentValidation) plug into the pipeline, and step-by-step how a request flows through the code from endpoint → MediatR → handler → database → response. See [Article2_Project_Code_And_Implementation.md](Article2_Project_Code_And_Implementation.md). |

**Suggested order:** Read Article 1 first for the concepts, then Article 2 to see them applied in code.

---

## The Project We're Building

We use one concrete example throughout both articles: a **Todo API**. The app lets users create, read, update, and delete **todo items** (each has a title, optional description, and a completed flag). It’s a small, familiar domain so we can focus on architecture instead of business rules.

Because the whole project is built around this **Todo** feature, you’ll see the word **Todo** everywhere: `Todo` entity, `CreateTodo` command, `GetTodo` query, `TodoEndpoints`, `Features/Todos/`, and so on. That’s intentional — one domain, one feature name, so the examples stay consistent and easy to follow. Once you see how Vertical Slice and CQRS work with Todos, you can apply the same ideas to any other feature (e.g. Orders, Invoices, Users).

---

## Table of Contents

- [The Project We're Building](#the-project-were-building)
1. [What is Vertical Slice Architecture?](#1-what-is-vertical-slice-architecture) — including [Where entities and DTOs live](#where-entities-and-dtos-live)
2. [What is CQRS?](#2-what-is-cqrs) — including [What is MediatR?](#what-is-mediatr), [Purpose of Command, Handler, and Validator](#purpose-of-command-handler-and-validator)
3. [Why They Work Well Together](#3-why-they-work-well-together)
4. [When to Use This Approach](#4-when-to-use-this-approach)
5. [When NOT to Use It](#5-when-not-to-use-it)
6. [Simple Diagram](#6-simple-diagram)
7. [Code Flow: Request → Response](#7-code-flow-request--response)
8. [Real-World Use Cases](#8-real-world-use-cases)
9. [Pros and Cons](#9-pros-and-cons)
10. [Comparison with Layered Architecture](#10-comparison-with-layered-architecture)

---

## 1. What is Vertical Slice Architecture?

**Vertical Slice Architecture** means organizing code **by feature** (or by “use case”), not by technical layer.

- **Traditional (horizontal/layered):** You have folders like `Controllers/`, `Services/`, `Repositories/`, `Models/`. One feature (e.g. “Create Todo”) is spread across many of these folders.
- **Vertical slice:** You have one folder per **feature** or per **action**. Everything needed for that action lives in one place: the request (command/query), the handler, the validator, and the endpoint mapping.

In this project, the “Todo” feature is split into **slices** like:

- `Features/Todos/CreateTodo/` — create a todo
- `Features/Todos/GetTodo/` — get one todo
- `Features/Todos/GetTodos/` — get all todos
- `Features/Todos/UpdateTodo/` — update a todo
- `Features/Todos/DeleteTodo/` — delete a todo

Each slice is **vertical** because it cuts through the “layers” (HTTP → application logic → data) in one small, self-contained place.

**In one sentence:** *You organize by “what the system does” (features/use cases), not by “what kind of class it is” (controllers, services, repositories).*

### Where entities and DTOs live

In this style of project:

- **Entities** (the classes that represent **database tables** and are used by EF Core) live in **Core**. All table definitions go there. Entities are shared across the app: handlers that write data create or update these objects and save them via the DbContext.
- **DTOs** (Data Transfer Objects — shapes used for **input** or **output** of an operation, not the table itself) live **with the feature**:
  - **Request DTOs** (what the client sends in the API, e.g. the body of POST/PUT) usually sit next to the endpoints or in the same feature folder.
  - **Result DTOs** (what a command or query returns, e.g. `CreateTodoResult`, `GetTodoResult`) live in the **same slice** as the command or query, often in the same file. Each slice owns its request/response shape.

So: **entities in Core, DTOs in the slice (or with the feature).** Article 2 names the actual files and types in this project.

---

## 2. What is CQRS?

**CQRS** stands for **Command Query Responsibility Segregation**.

- **Command** = an intention to **change** state (create, update, delete). Commands usually return a small result (e.g. id, or success).
- **Query** = an intention to **read** state. Queries return data and do not change it.

So you **separate**:

- **Writes** → Commands + Command Handlers  
- **Reads** → Queries + Query Handlers  

In this project:

- **Commands:** `CreateTodoCommand`, `UpdateTodoCommand`, `DeleteTodoCommand`  
- **Queries:** `GetTodoQuery`, `GetTodosQuery`  

Each command/query has **one handler** that contains the logic. MediatR is the library that “sends” the command or query to the right handler.

**In one sentence:** *You split “things that change data” (commands) from “things that read data” (queries), and give each a dedicated handler.*

---

### What is MediatR?

**MediatR** is a .NET library that implements the **mediator pattern**. In plain terms:

- Your endpoint (or controller) does **not** call the handler directly. Instead, it creates a command or query object and sends it to **MediatR** (e.g. `mediator.Send(command)`).
- MediatR looks at the type of that object and finds the **one handler** registered for that type, then calls it and returns the result.
- So the endpoint stays thin: it only builds the request and sends it. MediatR is the "middleman" that delivers the request to the correct handler and gives you back the response.

**Why use it?** It keeps the HTTP layer decoupled from business logic: the endpoint does not need to know which handler runs or how it works. It also makes it easy to add **pipeline behaviors** (e.g. validation, logging) that run before or after every request. In this project we use MediatR to wire commands and queries to their handlers and to run validators before the handler.

### Purpose of Command, Handler, and Validator

In each slice you typically have three kinds of pieces. Here is what each is **for**:

- **Command (or Query)** — **Purpose:** Represents *what* the user or system wants to do. It is a small object that carries only the data needed for that action (e.g. "create a todo with this title and description"). It does not contain logic. Commands are for writes; queries are for reads. The endpoint creates a command/query and sends it to MediatR; MediatR uses it to find the right handler.
- **Handler** — **Purpose:** Contains the *business logic* for that action. It receives the command (or query), talks to the database or other services, and returns a result. One handler per command/query. Handlers are where the real work happens (create entity, save, return result).
- **Validator** — **Purpose:** Checks that the *input* of a command (or request) is valid *before* the handler runs. It answers: "Is the title present? Is it too long?" If validation fails, the handler is never called and the API can return a 400 with error messages. Validators keep validation rules in one place and out of the handler.

So in short: **Command/Query** = the request message; **Handler** = the logic that runs for it; **Validator** = optional guard that runs first and blocks bad input. Article 2 shows these with real code examples from the project.

---

## 3. Why They Work Well Together

- **Vertical slices** need a clear “action” per slice. CQRS gives you exactly that: one command or one query per action.
- Each slice can contain:
  - The **command or query** (the “request”)
  - The **handler** (the “do the work” part)
  - Optional **validator**
  - Endpoint that sends the command/query to MediatR  

So: **one slice = one use case = one command or one query + its handler.**  

They fit together because vertical slices are about organizing by use case, and CQRS is about modeling each use case as a single command or query.

---

## 4. When to Use This Approach

- You want **clear, feature-based structure** and easy navigation (“everything for Create Todo is in one folder”).
- You have **many different use cases** and want to add or change them without touching unrelated code.
- You want **consistent patterns**: every feature has a request (command/query), a handler, and an endpoint.
- Your team likes **CQRS** (separate read and write models/handlers) and you’re using or considering MediatR (or similar).

Good fit for: APIs with multiple features, microservice-style boundaries per feature, and teams that prefer “slice by feature” over “layer by type”.

---

## 5. When NOT to Use It

- **Very small or throwaway apps** where a single controller and a few methods are enough; the extra structure might feel like overkill.
- **Strictly CRUD-only** apps with almost no business rules; a simple layered or minimal API can be enough.
- When the team **doesn’t want** MediatR or CQRS; then vertical slices can still be used without CQRS (e.g. one handler per endpoint, without a mediator).

So: use it when the benefits of slices + CQRS (structure, scalability, testability) matter more than the extra files and concepts.

---

## 6. Simple Diagram

```
                    CLIENT (e.g. Browser / Postman)
                                    |
                                    | HTTP (GET /api/todos/1)
                                    v
+----------------------------------------------------------------------------------+
|                         ASP.NET CORE (Minimal API)                               |
|  Endpoint: GET /api/todos/{id}  -->  mediator.Send(new GetTodoQuery(id))         |
+----------------------------------------------------------------------------------+
                                    |
                                    | MediatR finds handler for GetTodoQuery
                                    v
+----------------------------------------------------------------------------------+
|  VERTICAL SLICE: Features/Todos/GetTodo/                                         |
|  - GetTodoQuery.cs       (the query)                                             |
|  - GetTodoHandler.cs     (loads from DB, returns GetTodoResult)                  |
+----------------------------------------------------------------------------------+
                                    |
                                    | Handler uses AppDbContext
                                    v
+----------------------------------------------------------------------------------+
|  Infrastructure: AppDbContext (EF Core InMemory)                                 |
|  - Todos DbSet                                                                   |
+----------------------------------------------------------------------------------+
                                    |
                                    | Result flows back
                                    v
                    CLIENT receives JSON response (e.g. 200 OK + todo)
```

**Same flow for a command (e.g. POST create):**

- Request hits **endpoint** → endpoint builds **CreateTodoCommand** → **MediatR** sends it.
- **ValidationBehavior** runs **CreateTodoValidator** (if any); if invalid, returns 400.
- **CreateTodoHandler** runs → creates entity, saves via **DbContext** → returns **CreateTodoResult**.
- Endpoint returns **201 Created** with the result.

---

## 7. Code Flow: Request → Response

Step-by-step for **GET /api/todos/1** (get one todo):

1. **Request** hits the Minimal API route `GET /api/todos/{id}` (mapped in `TodoEndpoints`).
2. **Endpoint** receives `id`, calls `mediator.Send(new GetTodoQuery(id))`.
3. **MediatR** finds the handler that implements `IRequestHandler<GetTodoQuery, GetTodoResult?>` → `GetTodoHandler`.
4. **GetTodoHandler** runs: uses `AppDbContext`, queries `Todos`, returns a `GetTodoResult` or `null`.
5. **MediatR** returns that result to the endpoint.
6. **Endpoint** returns `Results.Ok(result)` or `Results.NotFound()`.
7. **Response** is sent to the client as JSON.

For **POST /api/todos** (create todo):

1. **Request** hits `POST /api/todos` with body `{ "title": "...", "description": "..." }`.
2. **Endpoint** builds `CreateTodoCommand(title, description)` and calls `mediator.Send(command)`.
3. **MediatR** pipeline runs **ValidationBehavior** first → **CreateTodoValidator** runs; if invalid, throws `ValidationException` → exception handler returns **400** with errors.
4. If valid, **CreateTodoHandler** runs: creates `Todo`, adds to `DbContext`, `SaveChangesAsync`, returns `CreateTodoResult`.
5. **Endpoint** returns **201 Created** with the result and `Location` header.

So in general: **Request → Endpoint → MediatR (optional validation) → Handler → DB (if needed) → Result → Response.**

---

## 8. Real-World Use Cases

- **SaaS back-office:** Many features (invoicing, users, reports). Each feature is a vertical slice; each action is a command or query.
- **E-commerce:** “Place order” (command), “Get order status” (query), “List my orders” (query). Each can be a slice and can evolve independently.
- **Internal tools:** Lots of small use cases (approve, reject, export, filter). Slices keep each use case in one place and make it easy to add new ones.

The idea: whenever you have **many distinct actions** and want **clear boundaries and scalability**, vertical slices + CQRS are a good fit.

---

## 9. Pros and Cons

**Pros**

- **Easy to find code:** “Where is create todo?” → `Features/Todos/CreateTodo/`.
- **Easy to add features:** New feature = new folder with command/query, handler, optional validator, and one endpoint.
- **Clear separation of reads and writes** (CQRS).
- **Testable:** Handlers can be unit-tested by sending commands/queries and mocking the DbContext.
- **Less “layer sprawl”:** No giant `Services` or `Repositories` folders where every feature is mixed together.

**Cons**

- **More files and folders** than a single controller with a few methods.
- **Concepts to learn:** MediatR, CQRS, pipeline behaviors, optional FluentValidation.
- **Can be overkill** for very small or short-lived projects.

---

## 10. Comparison with Controller / Layered Architecture

### How controller or layered architecture works

In a **controller-based** or **layered** setup you organize by **technical role**:

- **Controllers** — Handle HTTP: one controller per resource (e.g. `TodosController`). Each action (Create, Get, Update, Delete) is a method that receives the request, calls a service, and returns a response.
- **Services** — Hold business logic: e.g. `TodoService` with methods like `CreateTodo()`, `GetTodo()`, `GetAllTodos()`. The controller calls the service; the service may call a repository.
- **Repositories** — Handle data access: e.g. `TodoRepository` with `Add()`, `GetById()`, etc. The service calls the repository.

So for **one feature** (e.g. "create a todo") the code lives in **several places**: the controller action, the service method, the repository method, and maybe DTOs in a shared folder. To understand or change "create todo" you open multiple files across different layers.

### How vertical slice + CQRS is different

Here you organize by **use case**, not by role:

- There is no single "TodoController" or "TodoService" that owns all todo operations. Instead, **each action** has its own small slice.
- "Create todo" = one folder (`CreateTodo/`) with the **command** (the request), the **handler** (the logic — what would be in the service method), and optionally a **validator**. The **endpoint** (Minimal API) only builds the command and sends it to MediatR; it does not call a service or repository directly.
- So **one feature = one place**. To change "create todo" you go to `Features/Todos/CreateTodo/`. No jumping between Controllers, Services, and Repositories.

**Difference in one line:** In layered you ask "which controller, which service, which repository?"; in vertical slice you ask "which slice?" and everything for that use case is there.

### Quick comparison table

| Aspect            | Layered (horizontal)           | Vertical Slice + CQRS (this project)     |
|------------------|---------------------------------|------------------------------------------|
| **Organization** | By technical role (Controller, Service, Repository) | By feature/use case (CreateTodo, GetTodo, …) |
| **Adding a feature** | Often touch Controller, Service, maybe Repository, DTOs | Add one folder (e.g. CreateTodo) with command, handler, endpoint |
| **Finding code** | “Create todo” logic spread across layers | “Create todo” in `Features/Todos/CreateTodo/` |
| **Read vs write** | Often same service/repository for both | Commands vs queries and handlers separated (CQRS) |
| **Best for**     | Simple CRUD, small teams, straightforward domains | Growing APIs, many features, teams that like feature-based structure |

Both are valid. Layered is simpler for very small apps; vertical slice + CQRS keeps each use case in one place and scales better when you have many features. Article 2 shows the same "create todo" in both styles with short code examples.

---

## Summary

- **Vertical Slice Architecture** = organize by **feature/use case** (one folder per action or feature).
- **CQRS** = separate **commands** (write) from **queries** (read), each with its own handler.
- In this project, **one slice = one command or query + handler + optional validator + endpoint**; MediatR connects endpoints to handlers.
- **Flow:** Request → Endpoint → MediatR (validation if configured) → Handler → DbContext → Result → Response.

Use this approach when the benefits of clear structure and CQRS matter; skip it or simplify when the project is very small or the team prefers fewer concepts and files.
