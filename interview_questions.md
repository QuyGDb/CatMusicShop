# MusicShop — .NET Backend Interview Questions

Questions grounded in **your actual codebase**. Organized from architectural to implementation-level.

---

## 1. Clean Architecture & Project Structure

**Q1.1** Your solution has four layers: `Domain → Application → Infrastructure → Api`. What is the **Dependency Rule** and how does your project enforce it? What would break if `Domain` referenced `Infrastructure`?

**Q1.2** `IRepository<T>` lives in `Domain/Interfaces`, but its implementation `GenericRepository<T>` lives in `Infrastructure/Persistence`. Why not put the interface in `Application` instead? What's the trade-off?

**Q1.3** You have specialized repositories like `IArtistRepository`, `IProductRepository` alongside the generic `IRepository<T>`. When do you create a specialized repository vs. relying on the generic one? Give a concrete example from your codebase.

**Q1.4** Your `Application` layer has both `UseCases/` and `Handlers/` directories. How do you decide where a piece of logic belongs? What's the boundary between a "use case" and a "handler"?

---

## 2. Domain Modeling

**Q2.1** `Order.TransitionTo(OrderStatus)` uses a **state machine** with pattern matching:
```csharp
bool isValid = (Status, targetStatus) switch
{
    (OrderStatus.Pending, OrderStatus.Confirmed) => true,
    (OrderStatus.Shipped, OrderStatus.Delivered) => true,
    ...
    _ => false
};
```
Why is this logic on the entity rather than in a handler? What happens if you need to add a new status like `Refunded` — what parts of the codebase need to change?

**Q2.2** `Product` has navigation properties for `VinylAttributes`, `CdAttributes`, and `CassetteAttributes`. This looks like a **Table-Per-Hierarchy (TPH)** or optional one-to-one approach. What are the trade-offs vs. using TPH inheritance with a discriminator column? Why did you choose this design?

**Q2.3** Your `BaseEntity` has `CreatedAt` and `UpdatedAt`. In `Order.TransitionTo()`, you manually set `UpdatedAt = DateTime.UtcNow`. What's the risk of this approach vs. handling it automatically in `SaveChangesAsync` via an EF Core interceptor or `ChangeTracker`?

**Q2.4** `Product` has both `IsAvailable` and `StockQty`. Under what conditions could these be inconsistent? Should `IsAvailable` be a computed property derived from `StockQty > 0 && IsActive`?

---

## 3. Result Pattern & Error Handling

**Q3.1** Your `Result<T>` throws `InvalidOperationException` when accessing `.Value` on a failed result. Why is this acceptable despite the project's philosophy of avoiding exceptions for business logic?

**Q3.2** `ProductErrors` defines errors like `DuplicateSlug`, `InsufficientStock`, `HasPendingOrders` — each with an `ErrorType` (NotFound, Conflict, Validation). How does the API layer map `ErrorType` to HTTP status codes? Walk through the flow from handler → controller → HTTP response.

**Q3.3** Your `GlobalExceptionHandler` handles `ValidationException` (FluentValidation) and falls through to 500 for everything else. But `Result.Failure` **doesn't throw** — it returns a result. How do validation errors from FluentValidation and domain errors from `Result` reach the client differently?

**Q3.4** `Error` is a `sealed record`. Why `record` and not a `class`? What benefit does structural equality give you in error comparison?

---

## 4. CQRS & MediatR

**Q4.1** Your `ValidationBehavior<TRequest, TResponse>` runs validators via `Task.WhenAll`. What happens if a request has **no registered validators**? Why does the code short-circuit with `if (!validators.Any())`?

**Q4.2** The validation behavior throws `ValidationException` on failure. This means FluentValidation errors are exceptions, but domain errors use `Result`. Is this inconsistent? How would you unify them?

**Q4.3** You separate use cases into `Commands/` and `Queries/` directories. What prevents a "Query" handler from modifying state? Is this enforced at compile time or by convention?

**Q4.4** If you needed to add **logging** and **authorization** as cross-cutting concerns to every handler, how would you implement them using MediatR's pipeline? What's the order of execution relative to `ValidationBehavior`?

---

## 5. EF Core & Persistence

**Q5.1** `IRepository<T>` exposes `Expression<Func<T, bool>>` predicates via `FirstOrDefaultAsync` and `AnyAsync`. This leaks LINQ expression trees into the Application layer. Is that a problem? How would you refactor it if you wanted to keep the Application layer completely ORM-agnostic?

**Q5.2** Your `UnitOfWork` wraps `AppDbContext.SaveChangesAsync`. Who calls `SaveChangesAsync` — the handler or the repository? What's the rationale?

**Q5.3** In `DependencyInjection.cs`, you enable `EnableSensitiveDataLogging()` in development. What does this log, and why would it be a **security risk** in production?

**Q5.4** `GetByIdsAsync(IEnumerable<Guid> ids)` — how does EF Core translate this to SQL? What happens if you pass 10,000 IDs? How would you handle that?

**Q5.5** You use `AsNoTracking()` for read queries. What specific problem does this solve? What breaks if you accidentally call `Update()` on an untracked entity?

---

## 6. Authentication & Security

**Q6.1** Your JWT configuration sets `MapInboundClaims = false`. What does this do, and what subtle bug does it prevent with claim names?

**Q6.2** `IPasswordHasher` and `IRefreshTokenHasher` are registered as **Singleton**. Why is a hasher safe as a singleton, but a `DbContext` is not?

**Q6.3** You set `NameClaimType = "name"` and `RoleClaimType = "role"` in `TokenValidationParameters`. What happens if you forget this and ASP.NET Core tries to read roles from the default `ClaimTypes.Role` URI?

**Q6.4** The refresh token flow: walk through what happens when an access token expires. How do you prevent **refresh token replay attacks**? Does your `RefreshToken` entity hash the token?

**Q6.5** You have Google OAuth via `IGoogleAuthService`. How do you handle the case where a user signs up with email/password first, then later tries to sign in with Google using the same email?

---

## 7. Stripe Payment Integration

**Q7.1** `StripeService.CreateCheckoutSessionAsync` sets `ExpiresAt = DateTime.UtcNow.AddMinutes(30)`. What happens to the `Order` if the session expires without payment? Who cleans up pending unpaid orders?

**Q7.2** Your webhook handler returns `WebhookProcessResult` instead of directly updating the order. Why this indirection? What would go wrong if the webhook handler directly called `SaveChangesAsync`?

**Q7.3** `ConvertToStripeAmount` uses `Math.Round(price * 100, MidpointRounding.AwayFromZero)`. Why `AwayFromZero` specifically? What's the risk with the default `MidpointRounding.ToEven` for currency?

**Q7.4** Stripe webhooks can arrive **before** your redirect success page loads. How does your architecture handle this race condition between the webhook confirming payment and the frontend polling for order status?

**Q7.5** `RefundOrderAsync` retrieves the session, gets `PaymentIntentId`, then creates a refund. What happens if the payment was only **partially** captured? Does your code handle partial refunds?

---

## 8. Outbox Pattern & Messaging

**Q8.1** Your `MessageProcessor` uses a **pessimistic lock** via raw SQL:
```sql
UPDATE "Messages" SET "LockId" = {0}
WHERE "Id" = {1} AND "LockId" IS NULL AND "ProcessedAt" IS NULL
```
Why raw SQL instead of EF Core's concurrency tokens (`[ConcurrencyCheck]` / `RowVersion`)? What's the trade-off?

**Q8.2** The `Deserialize` method maps string message types to CLR types via a hardcoded dictionary. What happens when you add a new event type but forget to update this map? How would you make this more resilient?

**Q8.3** `MessagePollingJob` runs every 2 minutes via Hangfire. What are the trade-offs of polling vs. using PostgreSQL's `LISTEN/NOTIFY` for real-time processing?

**Q8.4** You have both **Outbox** (app → external) and **Inbox** (external → app) message directions. Walk through the complete lifecycle of a Stripe payment: webhook received → inbox message created → processed → order updated. What guarantees does this give you?

**Q8.5** `[AutomaticRetry(Attempts = 5, DelaysInSeconds = new[] { 10, 30, 60, 120, 300 })]` — what happens after the 5th retry fails? Is the message permanently lost? How would you implement a dead-letter queue?

---

## 9. Dependency Injection

**Q9.1** `IPasswordHasher` is Singleton, `ICurrentUserService` is Scoped. What happens if you inject `ICurrentUserService` into a Singleton service? What's this anti-pattern called?

**Q9.2** You register `IRepository<>` as an open generic: `services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>))`. But you also register specific repositories like `IProductRepository`. Which registration wins when you resolve `IRepository<Product>`? What about `IProductRepository`?

**Q9.3** `StripeServiceRegistration` is a separate extension method. Why not register Stripe in the main `DependencyInjection.cs`? What's the benefit of this separation?

**Q9.4** You use `ValidateOnStart()` for `JwtSettings`, `GoogleSettings`, `AdminSettings`. What happens at app startup if a required config value is missing? How is this better than discovering the error at runtime?

---

## 10. API Design & Middleware

**Q10.1** Your `GlobalExceptionHandler` exposes `exception.Message` in the 500 response:
```csharp
problemDetails.Detail = exception.Message;
```
Why is this a security concern in production? What should you return instead?

**Q10.2** You configure CORS with `AllowCredentials()` and specific origins. Why can't you use `AllowAnyOrigin()` together with `AllowCredentials()`? What's the browser security model behind this?

**Q10.3** You set `Cross-Origin-Opener-Policy: same-origin-allow-popups` header. What attack does this mitigate? Why `same-origin-allow-popups` instead of strict `same-origin`?

**Q10.4** `ForwardedHeaders` is configured for `XForwardedFor | XForwardedProto`. In your Docker/Nginx deployment, what breaks if you forget this middleware?

**Q10.5** `public partial class Program { }` at the bottom of `Program.cs` — what is this for? How does it relate to integration testing with `WebApplicationFactory<Program>`?

---

## 11. AWS S3 & File Storage

**Q11.1** `IImageService` is in `Application/Common/Interfaces` but the implementation `S3ImageService` is in `Infrastructure/Storage`. If you wanted to swap S3 for Azure Blob Storage, what would you need to change?

**Q11.2** You use `BasicAWSCredentials` with explicit access key and secret from config. What's the recommended approach in a production AWS environment (ECS/EC2) vs. local development?

**Q11.3** Your `UploadImageCommand` is a use case. Should image upload validation (file size, MIME type, dimensions) happen in the validator, the handler, or the `IImageService` implementation? Why?

---

## 12. Background Jobs (Hangfire)

**Q12.1** You configure Hangfire to use PostgreSQL storage — the **same database** as your application. What are the performance implications? When would you use a separate database?

**Q12.2** `MessagePollingJob` is registered as **Scoped**, but Hangfire creates its own scope per job execution. What happens to the `DbContext` injected into the job — is it the same instance as the one used by the `MessageProcessor`?

**Q12.3** You protect the Hangfire dashboard with `HangfireAdminAuthorizationFilter`. What's at risk if the dashboard is exposed without authentication in production?

---

## 13. Testing & Quality

**Q13.1** Your `Program.cs` ends with `public partial class Program { }`. Write a skeleton integration test using `WebApplicationFactory<Program>` that verifies the `/api/v1/products` endpoint returns 200.

**Q13.2** `Order.TransitionTo()` is pure domain logic with no external dependencies. Write three unit test cases that cover: valid transition, invalid transition, and same-status no-op.

**Q13.3** How would you test `StripeService.CreateCheckoutSessionAsync` without hitting the real Stripe API? What mocking strategy would you use?

**Q13.4** Your `ValidationBehavior` runs all validators in parallel via `Task.WhenAll`. Is there a scenario where parallel validation could produce **inconsistent results** (e.g., two validators checking the same DB row)?

---

## 14. Performance & Scalability

**Q14.1** `PaginatedResult<T>` computes `TotalPages` on every access: `(int)Math.Ceiling(TotalCount / (double)PageSize)`. Is this a problem? What about the `COUNT(*)` query to get `TotalCount` on large tables?

**Q14.2** Your `IRepository<T>` doesn't support `Include()` for eager loading. How do your specialized repositories (e.g., `IOrderRepository`) handle loading `OrderItems` and `Payment` navigation properties?

**Q14.3** If your product catalog grows to 1M+ products, what would you add to the `ProductRepository` to support full-text search? Would you use PostgreSQL's `tsvector` or an external service like Elasticsearch?

---

## 15. Deployment & DevOps

**Q15.1** Your `Dockerfile` is in the API project. Walk through the multi-stage build: what happens in each stage and why don't you copy the entire solution in the final image?

**Q15.2** Database migrations: how do you apply EF Core migrations in production? Do you run them at app startup, in a CI/CD pipeline, or via a separate migration job? What's the risk of running them at startup in a multi-instance deployment?

**Q15.3** Your `appsettings.json` contains connection strings and JWT settings. How do you manage secrets in production — environment variables, AWS Secrets Manager, or something else?

---

## Rapid-Fire (Short Answer)

| # | Question |
|---|----------|
| 1 | Why `sealed` on `StripeService` and `GlobalExceptionHandler`? |
| 2 | Why `IReadOnlyList<T>` instead of `List<T>` for repository return types? |
| 3 | What's the difference between `AddScoped` and `AddTransient` for repositories? |
| 4 | Why `CancellationToken` on every async method? |
| 5 | Why `JsonStringEnumConverter` in controller JSON options? |
| 6 | What does `UseForwardedHeaders` do behind a reverse proxy? |
| 7 | Why `record` for `Error` but `class` for `Result`? |
| 8 | What's the purpose of the `Slug` property on `Product`? |
| 9 | Why is `SaveChangesAsync` in the handler, not in the repository? |
| 10 | What happens if two users add the last item to cart simultaneously? |
