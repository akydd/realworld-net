# realworld-net

A backend implementation of the [RealWorld](https://realworld-docs.netlify.app/) ("Conduit") API spec, built with **ASP.NET Core on .NET 10**. RealWorld is a Medium.com-style blogging platform — users, profiles with following, articles with slugs and favorites — used as a reference spec for exercising a framework end to end.

> **Status: in progress.** The users, profiles, and articles feature sets are implemented (including auth, following, and favorites). Comments, tags, and the personal feed are not yet built — see [Implementation status](#implementation-status).

## Tech stack

- **ASP.NET Core** (controllers) on **.NET 10**
- **Entity Framework Core 10** with **SQL Server** (running in Docker)
- **JWT bearer authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`), issued with HS256
- **OpenAPI** via the built-in `Microsoft.AspNetCore.OpenApi`, with a **[Scalar](https://scalar.com/)** API reference UI
- Nullable reference types, analyzers, and `TreatWarningsAsErrors` enabled; formatting enforced via `.editorconfig` + `dotnet format`

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for the SQL Server container)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

### Run it

SQL Server must be running before the app or EF migrations will work.

```bash
docker compose up -d        # SQL Server 2022 on localhost:1433
dotnet ef database update   # apply migrations
dotnet run                  # http://localhost:5268, https://localhost:7215
```

In development, the OpenAPI document is served at `/openapi/v1.json` and the interactive **Scalar UI at `/scalar`** — the easiest way to explore and exercise the endpoints.

### Configuration & secrets

Local settings (connection string, JWT signing key) live in `appsettings.Development.json` for convenience. The JWT secret there is a placeholder; for anything beyond local dev, move it out of source with [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets set "JwtSettings:Secret" "<a-long-random-key>"
```

## API

The API follows the [RealWorld endpoint spec](https://realworld-docs.netlify.app/specifications/backend/endpoints/). Authenticated requests use the RealWorld scheme **`Authorization: Token <jwt>`** (not `Bearer`). In the Scalar UI, use the **Authorize** control and paste the full `Token <jwt>` value.

### Implementation status

| Feature | Endpoint | Status |
|---|---|---|
| Register | `POST /api/users` | ✅ |
| Login | `POST /api/users/login` | ✅ |
| Current user | `GET /api/user` | ✅ |
| Update user | `PUT /api/user` | ✅ |
| Get profile | `GET /api/profiles/{username}` | ✅ (optional auth) |
| Follow / unfollow | `POST`/`DELETE /api/profiles/{username}/follow` | ✅ |
| Create article | `POST /api/articles` | ✅ |
| List articles (filters) | `GET /api/articles` | ✅ |
| Get article | `GET /api/articles/{slug}` | ✅ |
| Update article | `PUT /api/articles/{slug}` | ✅ |
| Delete article | `DELETE /api/articles/{slug}` | ✅ |
| Favorite / unfavorite | `POST`/`DELETE /api/articles/{slug}/favorite` | ✅ |
| Personal feed | `GET /api/articles/feed` | ⬜ Not yet |
| Comments | `.../comments` | ⬜ Not yet |
| Tags | `GET /api/tags` | ⬜ Not yet |

## Project structure

```
Controllers/   HTTP endpoints (Users, User, Profiles, Articles)
Services/      Business logic; each service is interface-backed and injected with the DbContext
Entities/      EF Core-mapped classes (User, Article, Follows, Favorites, Auditable base)
Models/        API-facing DTOs returned by services (records)
Dtos/          Request/response payload shapes matching the RealWorld JSON contract
Middleware/    Global exception handlers (IExceptionHandler)
Data/          AppDbContext (change-tracked timestamps, relationship config)
Migrations/    EF Core migrations
```

The request flow is a conventional layered pipeline: **Controller → `IXxxService` → `AppDbContext` (EF Core / SQL Server)**. Services return `Models` DTOs (never entities), and read paths use LINQ **projections** so only the needed columns are queried.

## Design decisions

### Authentication: hand-rolled `User` table, not `IdentityDbContext`

This project stores users in its own `User` table with a `PasswordHash` column and issues JWTs directly, rather than adopting ASP.NET Core Identity's `IdentityDbContext`. The reasoning:

- **The RealWorld contract doesn't need what Identity provides.** `IdentityDbContext` brings ~7 tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserClaims`, `AspNetUserRoles`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetRoleClaims`). RealWorld has no roles, external logins, claims tables, email confirmation, or lockout — those tables would sit empty.

- **Identity is built around cookie-based sign-in.** Its `UserManager`/`SignInManager` pipeline defaults to cookie auth, whereas RealWorld is a stateless **JWT bearer** API returning a fixed `{ user: { email, token, username, bio, image } }` shape. Using Identity would mean bending it away from its defaults.

- **The entity stays clean.** `IdentityUser` defaults to a string/GUID key and columns we'd never use (`NormalizedUserName`, `SecurityStamp`, `PhoneNumber`, `TwoFactorEnabled`, …). Our `Entities.User` is an `int Id` plus exactly the RealWorld fields.

**What is borrowed instead:** only Identity's password hasher. `Services.PasswordHasher` wraps `PasswordHasher<object>` (PBKDF2, salted, versioned) — the hashing algorithm without the DbContext or schema. That type ships in the ASP.NET Core shared framework (`Microsoft.AspNetCore.App`), so no extra NuGet package is needed. JWTs are issued by `Services.JWTService`.

The token returned to clients is **not** a DB column — it's derived per request by signing the user's id. Only the password hash is persisted.

### Favorites count is denormalized

`Article.FavoritesCount` is stored on the article rather than counted from the join table on every read. Favorite/unfavorite therefore update the counter and the `Favorites` row inside a single transaction, using an atomic SQL increment (`FavoritesCount + 1`) to stay correct under concurrency.

## Roadmap

- Personal feed (`GET /api/articles/feed`) over followed authors
- Comments and tags
- Automated tests (unit + integration via `WebApplicationFactory`) and a CI workflow
