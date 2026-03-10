# Prompt — Mini Projeto de Aprendizado Fullstack .NET + React

Cole este prompt inteiro ao iniciar uma nova conversa na IDE.

---

## Quem sou eu

Rodrigo Rocha, Desenvolvedor Fullstack .NET com ~4 anos de experiência.

**O que já sei:**
- C#/.NET 8, EF Core, PostgreSQL, REST APIs, xUnit/Moq
- Vue 3 (básico — Monitoramento Escolar com Tailwind + Chart.js)
- Docker básico (docker-compose up, mas não entendo por baixo)
- CI/CD com Azure DevOps, Git com Gitflow
- OAuth2/OIDC (uso Keycloak, mas não entendo profundamente)

**O que preciso aprender (gaps para Pleno):**
- Arquitetura de Software com profundidade (Clean Architecture, DDD light)
- SOLID e Clean Code na prática (não só teoria)
- Design Patterns aplicados (não decorar GoF, mas usar no código)
- Mensageria (RabbitMQ — não sei nada)
- Caching com Redis (já existia no trabalho, nunca implementei)
- Auth do zero (Identity + JWT — entender os fundamentos)
- Docker de verdade (Dockerfile, multi-stage, volumes, networks)
- Testes de integração robustos (WebApplicationFactory, Testcontainers)
- **React + Next.js** (migrar de Vue para o framework dominante do mercado)

**Objetivo:** Construir um projeto de portfólio que exercite todos esses conceitos. Quero que você me ENSINE enquanto construímos juntos — explique o porquê de cada decisão, qual princípio SOLID está sendo aplicado, qual Design Pattern está em uso.

---

## Setup do Projeto

- **IDE:** VS Code (com extensões C# Dev Kit + ES7+ React/Redux/React-Native snippets + Tailwind CSS IntelliSense)
- **Repositório:** Criar repo Git novo chamado `miniauth-blog` no GitHub
- **Pasta:** `C:\Users\Rodrigo\source\repos\blogNET`
- **.NET SDK:** 8.0
- **Node:** 20+
- **Package manager:** pnpm (frontend)

---

## O Projeto: MiniAuth Blog Platform

Domínio simples (blog) para focar nos patterns, não na complexidade de negócio.

### Architectural approach: Controller → Service → Repository

**This project uses the classic layered pattern**, which is the most common in the .NET job market, especially in companies with existing codebases. This is the pattern most developers encounter daily:

```
Controller (receives HTTP request)
    → Service (business logic, validation, orchestration)
        → Repository (data access, EF Core queries)
            → Entity (domain model)
```

**Why this instead of MediatR/CQRS:**
- It's what 80%+ of .NET projects in the market actually use
- Simpler to understand, debug, and maintain for a small-to-medium project
- MediatR/CQRS adds value in large projects with many developers — for a blog with 3 entities it's overengineering
- You learn the foundation FIRST, then can evolve to CQRS later (see Bonus Phase 10)
- In job interviews, they'll ask about services and repositories, not about MediatR handlers

### Features

| Feature | O que exercita |
|---------|---------------|
| Registro + Login | ASP.NET Core Identity, JWT (access + refresh token), password hashing |
| Roles (admin/autor/leitor) | Claims-based authorization, policies |
| CRUD de Posts | Clean Architecture (Controller → Service → Repository) |
| Comentários em Posts | Relacionamentos EF Core, validação, domain events |
| Notificação por email | RabbitMQ (comentário novo → evento → consumer envia email) |
| Cache de posts populares | Redis cache-aside pattern, invalidação |
| Paginação + Filtros | Query objects, IQueryable builder, PaginatedResult |
| Testes | Unit (xUnit/Moq) + Integration (WebApplicationFactory + Testcontainers) |
| Frontend React | Next.js App Router, Zustand, server/client components, route protection |
| Docker | Compose com PostgreSQL, Redis, RabbitMQ, MailHog |

### Arquitetura

```
miniauth-blog/
├── src/
│   ├── MiniAuth.Domain/           # net8.0 — Entities, Interfaces, Exceptions, Domain Events
│   ├── MiniAuth.Application/      # net8.0 — Services, DTOs, Interfaces, Validation
│   ├── MiniAuth.Infrastructure/   # net8.0 — EF Core, Repos, Redis, RabbitMQ, Email, DI Registration
│   ├── MiniAuth.Api/              # net8.0 Web — Controllers, Filters, Auth config, Middleware
│   └── MiniAuth.Worker/           # net8.0 Worker — RabbitMQ Consumer (envia emails)
│
├── web/                           # Next.js 14 (App Router)
│   ├── app/                       # App Router (layouts, pages, route groups)
│   │   ├── (auth)/                # Route group: login, register
│   │   ├── (dashboard)/           # Route group: posts, profile (protegidas)
│   │   └── layout.tsx             # Root layout
│   ├── components/                # React components
│   ├── hooks/                     # Custom hooks (useAuth, useApi)
│   ├── stores/                    # Zustand stores
│   ├── types/                     # TypeScript interfaces
│   ├── lib/                       # Utilities, API client, auth helpers
│   └── middleware.ts              # Next.js middleware (route protection)
│
├── tests/
│   ├── MiniAuth.UnitTests/        # xUnit + Moq + Bogus
│   └── MiniAuth.IntegrationTests/ # xUnit + WebApplicationFactory + Testcontainers
│
└── docker-compose.yml             # PostgreSQL + Redis + RabbitMQ + MailHog
```

### Referências de projeto (.csproj)

```
MiniAuth.Domain        → (nenhuma referência — camada pura)
MiniAuth.Application   → MiniAuth.Domain
MiniAuth.Infrastructure→ MiniAuth.Domain, MiniAuth.Application
MiniAuth.Api           → MiniAuth.Application, MiniAuth.Infrastructure
MiniAuth.Worker        → MiniAuth.Application, MiniAuth.Infrastructure
MiniAuth.UnitTests     → MiniAuth.Application, MiniAuth.Domain
MiniAuth.IntegrationTests → MiniAuth.Api, MiniAuth.Infrastructure
```

---

## Entidades do Domínio

### Entity base class
```csharp
public abstract class Entity {
    public Entity() { Id = Guid.NewGuid(); }
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```
- Auto-timestamps no `DbContext.SaveChanges()` via override de `BeforeSave()`
- Collections inicializadas no constructor como `HashSet<T>`
- Navigation properties `virtual` (lazy loading)
- `[JsonIgnore]` em referências circulares

### ApplicationUser (Identity)
```csharp
// Herda de IdentityUser (que já tem: Id, Email, PasswordHash, UserName, etc.)
public class ApplicationUser : IdentityUser {
    public string FullName { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
}
```

### Post
```csharp
public class Post : Entity {
    public string Title { get; set; }
    public string Content { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }

    public string AuthorId { get; set; }  // IdentityUser.Id is string
    public virtual ApplicationUser Author { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
}
```

### Comment
```csharp
public class Comment : Entity {
    public string Content { get; set; }

    public Guid PostId { get; set; }
    public virtual Post Post { get; set; }

    public string AuthorId { get; set; }
    public virtual ApplicationUser Author { get; set; }
}
```

### Repository interfaces (Domain)
```csharp
public interface IReadRepository<TEntity, TKey> : IDisposable where TEntity : class {
    ValueTask<TEntity> GetByIdAsync(TKey id, CancellationToken ct = default);
    IQueryable<TEntity> GetAll();
}

public interface IWriteRepository<TEntity> : IDisposable where TEntity : class {
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

### Service interfaces (Domain)
```csharp
// Interface in Domain — implementation in Application
public interface IPostService {
    Task<PostDto> CreateAsync(CreatePostRequest request, CancellationToken ct = default);
    Task<PostDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResult<PostDto>> ListAsync(PostQuery query, int page, int size, CancellationToken ct = default);
    Task<PostDto> UpdateAsync(Guid id, UpdatePostRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface ICommentService {
    Task<CommentDto> CreateAsync(Guid postId, CreateCommentRequest request, CancellationToken ct = default);
    Task<IEnumerable<CommentDto>> ListByPostAsync(Guid postId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

### Domain Exceptions
```csharp
public class DomainException : Exception {
    public List<string> Errors { get; set; }
    public DomainException(string message) : base(message) { Errors = new() { message }; }
    public DomainException(List<string> errors) : base(errors.First()) { Errors = errors; }
}

public class NotFoundException : Exception {
    public NotFoundException(string entity, object id)
        : base($"{entity} with id {id} was not found") { }
}
```

### Domain Events
```csharp
public abstract class DomainEvent {
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public class CommentCreatedEvent : DomainEvent {
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public string PostAuthorEmail { get; set; }
    public string CommenterName { get; set; }
    public string PostTitle { get; set; }
}
```

### Query objects
```csharp
public class PostQuery {
    public string? AuthorId { get; set; }
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
```

### Pagination
```csharp
public class PaginatedResult<T> where T : class {
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public IEnumerable<T> Data { get; set; }
}
```

---

## Architecture Patterns

### Data Layer

- `ApplicationDbContext : IdentityDbContext<ApplicationUser>` (inherits from Identity, not plain DbContext)
- `BeforeSave()` sets `CreatedAt`/`UpdatedAt` automatically on every entity
- EF Mappings via `IEntityTypeConfiguration<T>` (one class per entity)
- Abstract base map: `BaseMap<T>` configures PK, calls abstract `ConfigureEntity()` (Template Method pattern)
- Generic repository base (`EFRepositoryBase<TEntity, TKey>`) implements `IReadRepository` + `IWriteRepository`
- Concrete repositories with `.Include()` for eager loading and domain-specific queries
- PostgreSQL with `citext` extension for case-insensitive emails

### Application Layer (Services + DTOs + AutoMapper)

The classic service pattern: one service class per domain aggregate, injecting repositories and other services.

```csharp
// Service implementation — lives in Application layer
public class PostService(
    IPostRepository repository,
    IMapper mapper,
    ICurrentUser currentUser,
    IEventPublisher eventPublisher,
    ILogger<PostService> logger) : IPostService
{
    public async Task<PostDto> CreateAsync(CreatePostRequest request, CancellationToken ct)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new DomainException("Title is required");

        // Business logic
        var post = new Post
        {
            Title = request.Title,
            Content = request.Content,
            AuthorId = currentUser.UserId,
            IsPublished = false
        };

        repository.Add(post);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Post {PostId} created by {UserId}", post.Id, currentUser.UserId);
        return mapper.Map<PostDto>(post);
    }

    public async Task<PaginatedResult<PostDto>> ListAsync(PostQuery query, int page, int size, CancellationToken ct)
    {
        var queryable = repository.GetAll();

        // Apply filters from query object
        if (!string.IsNullOrEmpty(query.AuthorId))
            queryable = queryable.Where(p => p.AuthorId == query.AuthorId);
        if (query.IsPublished.HasValue)
            queryable = queryable.Where(p => p.IsPublished == query.IsPublished.Value);
        if (!string.IsNullOrEmpty(query.Title))
            queryable = queryable.Where(p => p.Title.Contains(query.Title));

        var total = await queryable.LongCountAsync(ct);
        var items = await queryable
            .OrderByDescending(p => p.CreatedAt)
            .Skip(page * size)
            .Take(size)
            .ToListAsync(ct);

        return new PaginatedResult<PostDto>
        {
            PageIndex = page,
            PageSize = size,
            TotalCount = total,
            Data = mapper.Map<IEnumerable<PostDto>>(items)
        };
    }
}
```

**Why this pattern:**
- It's what you'll find in most .NET codebases in the real world
- Easy to understand: Controller calls Service, Service calls Repository
- Easy to test: mock the repository, test the service logic
- Easy to debug: stack trace is linear, no magic dispatching
- Single Responsibility still applies: each service handles one aggregate

### DI Registration (Infrastructure, via extension methods)

```csharp
public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config) {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();

        services.AddStackExchangeRedisCache(options =>
            options.Configuration = config.GetConnectionString("Redis"));

        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        return services;
    }
}

public static class ApplicationServiceRegistration {
    public static IServiceCollection AddApplication(this IServiceCollection services) {
        // Services
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IAuthService, AuthService>();

        // AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}
```

Program.cs:
```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

### AutoMapper profiles (internal, inside Application)
```csharp
internal class PostProfile : Profile {
    public PostProfile() {
        CreateMap<Post, PostDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.FullName));
    }
}
```

### Api Layer

- `[ApiController]` + `[Route("api/v{version:apiVersion}/[controller]")]`
- Primary constructor injection (C# 12)
- `CancellationToken` on every action
- `IActionResult` as return type always
- `[ProducesResponseType]` on every action
- Global `HttpGlobalExceptionFilter` (DomainException → 400, NotFoundException → 404)
- Global `ValidateModelStateFilter`
- `ICurrentUser` abstraction for current user (reads JWT claims — interface in Domain, implementation in Api)
- `PaginatedResult<T>` for paginated responses

**Controller example — thin, delegates everything to service:**
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class PostsController(IPostService postService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<PostDto>), 200)]
    public async Task<IActionResult> List(
        [FromQuery] PostQuery query,
        [FromQuery] int page = 0,
        [FromQuery] int size = 20,
        CancellationToken ct = default)
    {
        var result = await postService.ListAsync(query, page, size, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PostDto), 201)]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request, CancellationToken ct)
    {
        var post = await postService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PostDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var post = await postService.GetByIdAsync(id, ct);
        return Ok(post);
    }
}
```

### Frontend (Next.js 14 — App Router + React)

**Stack:**
- Next.js 14 with App Router
- TypeScript
- Tailwind CSS
- Zustand (state management — simpler than Redux, similar concept to Pinia)
- Axios or native fetch with interceptors

**Patterns:**
- `app/` directory with route groups: `(auth)` for login/register, `(dashboard)` for protected pages
- `middleware.ts` at root for route protection (similar to Vue Router guards)
- Custom hooks: `useAuth`, `useApi`, `usePosts`
- Zustand store for auth state (token, user, login, logout)
- Server Components for static parts, Client Components for interactive parts
- `lib/api.ts` — centralized API client with JWT header injection
- Types in `/types/` — `User`, `Post`, `Comment`, `AuthTokens`

**Why React/Next.js over Vue/Nuxt:**
- React is the dominant framework in the Brazilian and global job market
- Next.js App Router is the current industry standard
- Learning React with a .NET backend is the most common fullstack combo in job postings
- Zustand ≈ Pinia (same concept, different ecosystem)
- Custom hooks ≈ Vue composables (same concept, different syntax)

---

## Design Patterns

Each feature must explicitly state which pattern is being used:

| Pattern | Where to apply |
|---------|---------------|
| **Repository** | `IPostRepository` → `PostRepository` |
| **Unit of Work** | `DbContext.SaveChangesAsync()` atomic |
| **Factory Method** | `Entity()` constructor generates `Id = Guid.NewGuid()` |
| **Observer** | Domain Events (CommentCreated → notifies author) |
| **Adapter** | RabbitMQ publisher adapts domain interface |
| **Decorator** | `CachedPostService` wraps `PostService` transparently |
| **Proxy** | EF Core Lazy Loading Proxies |
| **Builder** | IQueryable chain in repository and service |
| **Facade** | `AddInfrastructure()` hides N DI registrations in 1 call |
| **Strategy** | `IEventPublisher` interface — can swap RabbitMQ for another implementation |
| **Template Method** | `BaseMap<T>.Configure()` → `ConfigureEntity()` abstract |
| **Singleton** | `services.AddSingleton<>()` for RabbitMQ connection |

---

## SOLID Principles — Explain on each decision

| Principle | Example in project |
|-----------|-------------------|
| **S — Single Responsibility** | Controller only routes, Service handles business logic, Repository only persists |
| **O — Open/Closed** | New services can be added without modifying existing ones |
| **L — Liskov Substitution** | Any implementation of `IPostRepository` works in the service |
| **I — Interface Segregation** | `IReadRepository` separate from `IWriteRepository` |
| **D — Dependency Inversion** | Domain defines interfaces, Infrastructure implements them |

---

## Build Order (step by step)

I (Rodrigo) will TYPE the code. You guide me step by step, explaining BEFORE I write.

### Phase 1 — Foundation (Docker + Solution + Domain)
1. `docker-compose.yml` — PostgreSQL 16, Redis 7, RabbitMQ 3 (with management UI), MailHog
2. Test: `docker compose up -d` and verify everything is up (psql, redis-cli, RabbitMQ UI on port 15672, MailHog on 8025)
3. **Explain each service:** what it is, what it does in the project, what port it exposes and why
4. Create solution `MiniAuth.sln` with all `.csproj` and correct references
5. Domain: Entity base, ApplicationUser, Post, Comment, repository interfaces, service interfaces, exceptions, domain events, query objects
6. **Explain:** Dependency Inversion — why Domain references NOTHING else

### Phase 2 — Data (EF Core + Repositories)
7. Install packages: Npgsql.EntityFrameworkCore.PostgreSQL, Proxies
8. `ApplicationDbContext : IdentityDbContext<ApplicationUser>` with `BeforeSave()` (auto-timestamps)
9. EF Mappings: `PostMap`, `CommentMap` using `IEntityTypeConfiguration<T>`
10. Abstract base map `BaseMap<T>` (Template Method pattern)
11. Generic repository base `EFRepositoryBase<TEntity, TKey>`
12. Concrete repositories: `PostRepository`, `CommentRepository`
13. First migration: `dotnet ef migrations add Initial`
14. **Explain:** Repository Pattern, Unit of Work, why explicit mapping vs Data Annotations

### Phase 3 — Application (Services + DTOs + AutoMapper)
15. Install packages: AutoMapper.Extensions.Microsoft.DependencyInjection
16. DTOs: `PostDto`, `CommentDto`, `UserDto`
17. Request models: `CreatePostRequest`, `UpdatePostRequest`, `CreateCommentRequest`
18. `PostService` implementing `IPostService` — CRUD with validation and business logic
19. `CommentService` implementing `ICommentService` — with domain event publishing
20. AutoMapper profiles: `PostProfile`, `CommentProfile`
21. Extension method: `AddApplication()`
22. **Explain:** Service Pattern, why interface in Domain but implementation in Application, SRP in services

### Phase 4 — Api (Controllers + Auth)
23. `Program.cs` — build pipeline: `AddApplication()`, `AddInfrastructure()`, Identity, JWT
24. Identity setup with `ApplicationUser` + PostgreSQL (Npgsql)
25. JWT configuration: generate access token + refresh token, validation, expiration
26. `AuthController`: Register, Login, RefreshToken
27. `ICurrentUser` interface (Domain) + `CurrentUser` implementation (Api) — reads JWT claims
28. `PostsController` + `CommentsController` — thin controllers that call services
29. Global Filters: `HttpGlobalExceptionFilter`, `ValidateModelStateFilter`
30. Test everything via Swagger / curl
31. **Explain:** Facade, Strategy (auth schemes), middleware pipeline, why ICurrentUser abstracts HttpContext

### Phase 5 — Messaging (RabbitMQ)
32. Interface in Domain: `IEventPublisher` with `PublishAsync(DomainEvent event)`
33. Implementation in Infrastructure: `RabbitMqPublisher` (Adapter pattern)
34. In `CommentService.CreateAsync`: after saving, publish `CommentCreatedEvent`
35. Worker project: consumer that listens to queue, builds email, sends via SMTP to MailHog
36. Dead Letter Queue: message that fails 3x goes to separate queue
37. Test: create comment → see email in MailHog (http://localhost:8025)
38. **Explain:** Observer Pattern, Adapter, Producer/Consumer, Dead Letter, why async processing

### Phase 6 — Cache (Redis)
39. Install: Microsoft.Extensions.Caching.StackExchangeRedis
40. Interface: `ICacheService` with `GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`
41. Implementation: `RedisCacheService`
42. Decorator: `CachedPostService` wraps `PostService` — same interface, adds cache layer (Decorator pattern)
43. Cache-aside: check Redis → if empty call inner service (DB) → save to Redis with TTL
44. Invalidation: when creating/editing post, clear related cache
45. Test: first call slow (DB), second instant (cache)
46. **Explain:** Decorator Pattern, cache-aside, TTL, invalidation strategies

### Phase 7 — Tests
47. Unit tests: test services in isolation with Moq (mock repository, mapper, current user)
48. Unit tests: test domain exceptions and validation logic
49. Integration tests: `WebApplicationFactory<Program>` with Testcontainers (real PostgreSQL in Docker)
50. Integration tests: test complete flow Register → Login → Create Post → List
51. **Explain:** Test Pyramid, Arrange-Act-Assert, why Testcontainers > InMemory database

### Phase 8 — Frontend (Next.js + React)
52. `npx create-next-app@latest web --typescript --tailwind --app --src-dir` + install Zustand
53. Types: `AuthTokens`, `User`, `Post`, `Comment` in `/types/`
54. Zustand store: `useAuthStore` (token, user, login, logout, refresh)
55. API client: `lib/api.ts` — axios/fetch wrapper with JWT header injection and refresh logic
56. `middleware.ts` — Next.js middleware for route protection (redirect to /login if unauthenticated)
57. Layout: `app/(auth)/layout.tsx` (public) + `app/(dashboard)/layout.tsx` (protected, with sidebar)
58. Pages: `/login`, `/register`, `/` (posts list), `/posts/[id]` (detail + comments), `/posts/create`
59. Custom hooks: `useAuth()`, `usePosts()`, `useComments(postId)` — encapsulate API calls + state
60. **Explain:** React hooks vs Vue composables, Server vs Client Components, why Zustand over Redux, middleware pattern

### Phase 9 — Docker for real
61. `Dockerfile` for API — multi-stage build (build → publish → runtime)
62. `Dockerfile` for Worker — similar to API
63. `Dockerfile` for Web — build Next.js + serve with standalone output
64. Update `docker-compose.yml` with all services (infra + apps)
65. Test: `docker compose up` brings up EVERYTHING from scratch
66. **Explain:** layers, build cache, why multi-stage, Next.js standalone vs nginx

### BONUS Phase 10 — Evolve to MediatR/CQRS (optional, for portfolio)

After completing all phases above with the classic service pattern, refactor the Application layer to use MediatR and CQRS. This gives you:
- Knowledge of BOTH patterns (asked in senior interviews)
- A git history showing the evolution from one to the other (impressive for portfolio)
- Understanding of WHY CQRS exists and WHEN it makes sense

Steps:
67. Install MediatR and FluentValidation
68. Convert `CreatePostRequest` → `CreatePostCommand : IRequest<PostDto>`
69. Convert `PostService.CreateAsync` → `CreatePostHandler : IRequestHandler<CreatePostCommand, PostDto>`
70. Convert list queries → `GetPostsQuery : IRequest<PaginatedResult<PostDto>>`
71. Add Pipeline Behaviors: `ValidationBehavior`, `LoggingBehavior`
72. Refactor controllers to use `ISender` (MediatR) instead of services
73. **Explain:** Mediator Pattern, CQRS, Chain of Responsibility, when to use vs when it's overkill

---

## Rules for the assistant

1. **DO NOT write large code blocks at once.** Go step by step. Tell me what to do, I type.
2. **Explain WHY before HOW.** Before each file/class, explain: which SOLID principle applies, which design pattern, why this architectural decision.
3. **Use simple analogies** when explaining concepts. I learn better with concrete examples.
4. **Compare with the wrong way.** Show how it would look WITHOUT the pattern and why it's worse.
5. **When I make mistakes, don't silently fix them.** Explain the error and let me correct it.
6. **Ask me questions** like "what do you think should happen here?" before giving the answer.
7. **At the end of each phase,** give a summary of what I learned and a quick quiz (2-3 questions) to reinforce.
8. **Language:** Português BR, informal. Technical terms in English when they are the industry standard.
9. **Don't oversimplify.** This project is for learning real production patterns. If something is complex, explain the complexity instead of hiding it.
