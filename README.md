# AbySalto Mid — Technical Task

A small e-commerce backend exposing users, products, favorites, and shopping baskets. Products come from the [DummyJSON](https://dummyjson.com/) public API; everything else is persisted locally.

## Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core (Npgsql provider)
- PostgreSQL
- ASP.NET Core Identity + JWT bearer authentication with refresh tokens
- FluentValidation
- xUnit v3 + NSubstitute (SQLite in-memory for DB integration in unit tests)
- Swashbuckle / Swagger UI

## Prerequisites

- .NET 10 SDK
- PostgreSQL 14+ running locally (or any reachable Postgres instance)
- Optional: Visual Studio 2022/2026, Rider, or VS Code with the C# Dev Kit

## Getting started

1. Clone the repository.
2. Update the connection string in `AbySalto.Mid/appsettings.Development.json` if your local Postgres uses different credentials. The default expects `postgres/postgres@localhost:5432`.
3. Apply the EF Core migrations from the solution root:
   ```
   dotnet ef database update -p AbySalto.Mid.Infrastructure -s AbySalto.Mid
   ```
   The database `abysalto_mid` is created on first run.
4. Run the API. From Visual Studio choose the **https** launch profile, or from a terminal:
   ```
   dotnet run --project AbySalto.Mid --launch-profile https
   ```
5. Open Swagger UI at the root (`https://localhost:7221/`) — every endpoint is documented and the Authorize button supports the `Bearer <token>` flow.
6. To exercise endpoints without Swagger, the file `AbySalto.Mid/AbySalto.Mid.WebApi.http` runs in Visual Studio, Rider and VS Code and contains every request preconfigured.

## Project structure

```
AbySalto.Mid               WebApi host — controllers, middleware, Swagger, JWT wiring
AbySalto.Mid.Application   Use cases, DTOs, validators, service contracts and implementations
AbySalto.Mid.Domain        Entities and EF entity configurations (no external dependencies)
AbySalto.Mid.Infrastructure  EF Core, Identity, JWT issuance, DummyJSON HTTP client
tests/AbySalto.Mid.Application.Tests  Unit tests for service classes
```

Dependencies flow inward: Domain has no project references, Application depends only on Domain, Infrastructure depends on Application + Domain, WebApi composes them.

## Endpoints

All endpoints except register, login and refresh require a `Bearer` access token.

| Method | Route                                          | Description                              |
|--------|------------------------------------------------|------------------------------------------|
| POST   | `/v1/authentication/users`                     | Register a new user                      |
| POST   | `/v1/authentication/users/tokens`              | Login, returns access + refresh token    |
| POST   | `/v1/authentication/users/tokens/refresh`      | Exchange a refresh token for new tokens  |
| GET    | `/v1/authentication/users/me`                  | Current authenticated user               |
| GET    | `/v1/products?page=&pageSize=&sortBy=&order=`  | Paginated product list (cached)          |
| GET    | `/v1/products/{id}`                            | Single product (cached)                  |
| GET    | `/v1/favorites`                                | Current user's favorites, hydrated       |
| POST   | `/v1/favorites/{productId}`                    | Add a product to favorites               |
| DELETE | `/v1/favorites/{productId}`                    | Remove a product from favorites          |
| GET    | `/v1/basket`                                   | Current user's basket, hydrated          |
| POST   | `/v1/basket/items`                             | Add a product to the basket              |
| PUT    | `/v1/basket/items/{productId}`                 | Update the quantity of a basket item     |
| DELETE | `/v1/basket/items/{productId}`                 | Remove an item from the basket           |

## Architecture notes

A few decisions worth calling out beyond what the code shows:

- **`AddIdentityCore` over `AddDefaultIdentity`.** This is an API, not a Razor app — the cookie + UI flows that `AddDefaultIdentity` registers aren't needed. `AddIdentityCore` keeps the surface minimal.
- **Refresh tokens with two expirations.** The refresh token itself expires after 7 days (`RefreshTokenExpirationDays`), but a longer-lived `LoginExpiration` (30 days) caps how long a session can be kept alive across consecutive refreshes. After that the user re-authenticates with credentials.
- **DummyJSON adapter.** `IDummyJsonClient` exposes `ProductDto` only — the raw DummyJSON response shape is internal to Infrastructure. If DummyJSON changes its schema, only the adapter changes.
- **Caching keyed by tuple.** `IMemoryCache` keys for product lists include `(page, pageSize, sortBy, order)` so paginated queries don't collide. Missing products (`null` from DummyJSON) are intentionally not cached so a product added upstream later becomes discoverable without waiting for TTL.
- **`ServiceResult` + `IApplicationDbContext`.** Application services return `ServiceResult` / `ServiceResult<T>` and depend on an `IApplicationDbContext` abstraction defined in Application. Controllers translate results into HTTP responses through a small `ToActionResult` extension. Infrastructure implements the context; Application has no Infrastructure references.
- **Versioning via route prefix only.** All routes live under `/v1/`. `Asp.Versioning.*` was considered but adds ceremony for what `[Route("v1/...")]` already communicates at this scale.
- **Global exception middleware.** Catches anything that escapes a controller, returns RFC 7231 ProblemDetails. Stack traces are included only when the environment is `Development`.

## Testing

```
dotnet test
```

The test project covers the three application services that hold most of the logic:

- `ProductService` — caching behavior, including separate keys per (page, pageSize, sortBy, order) tuple, per-id caching, and the deliberate "do not cache nulls" policy.
- `FavoriteService` — happy paths plus multi-user isolation.
- `BasketService` — get-or-create lifecycle, conflict on duplicate add, validation, multi-user isolation, ordering.

Tests use a real `ApplicationDbContext` over SQLite in-memory (one isolated DB per test) so EF behavior — Include, unique constraints, ordering — is exercised. External dependencies (`IProductService` for Favorite/Basket tests, `IDummyJsonClient` for Product tests) are mocked with NSubstitute.

Controller-level and end-to-end integration tests are intentionally out of scope here. Service-level coverage gives high confidence with low maintenance overhead given the time budget.

## AI usage

This solution was built with AI assistance. Specifically: file scaffolding, boilerplate (csproj edits, DTO classes, validator boilerplate, repetitive controller actions), and translation of patterns from a familiar codebase into this project. Architectural choices and trade-offs — the `ServiceResult` / `IApplicationDbContext` split, the caching key strategy, the decision to use `AddIdentityCore`, the testing approach (SQLite in-memory + NSubstitute, no FluentAssertions due to its v8 license change), the choice not to ship integration tests within the time budget — are mine. Happy to walk through any decision on a call.

## Out of scope / next steps

A production version of this would add at least:

- Integration tests via `WebApplicationFactory<Program>` covering the JWT pipeline and auth-protected endpoints
- Refresh token rotation and revocation, and a logout endpoint
- Password reset and email confirmation flows (requires a mailer + framework reference to `Microsoft.AspNetCore.App` in Infrastructure)
- Rate limiting on auth endpoints
- A Dockerfile + docker-compose for the Postgres dependency
- Structured logging with Serilog and request correlation IDs
- Distributed cache (Redis) instead of `IMemoryCache` for multi-instance deployments
