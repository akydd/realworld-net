# realworld-net

[![CI](https://github.com/akydd/realworld-net/actions/workflows/dotnet-integration-tests.yaml/badge.svg)](https://github.com/akydd/realworld-net/actions/workflows/dotnet-integration-tests.yaml)

**🔗 [Live demo — interactive API docs](https://app-realworld-net-xia228.azurewebsites.net/scalar)** (Azure free tier; the first request after idle may take ~30–60s to wake)

A backend implementation of the [RealWorld](https://realworld-docs.netlify.app/) ("Conduit") API spec, built with **ASP.NET Core on .NET 10**. RealWorld is a Medium.com-style blogging platform — users, profiles with following, articles with slugs and favorites — used as a reference spec for exercising a framework end to end.

> **Status: in progress.** The users, profiles, and articles feature sets are implemented (including auth, following, and favorites). Comments, tags, and the personal feed are not yet built — see [Implementation status](#implementation-status).

## Tech stack

- **ASP.NET Core** (controllers) on **.NET 10**
- **Entity Framework Core 10** with **SQL Server** (running in Docker)
- **JWT bearer authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`), issued with HS256
- **OpenAPI** via the built-in `Microsoft.AspNetCore.OpenApi`, with a **[Scalar](https://scalar.com/)** API reference UI
- **Integration tests** with **xUnit** + **[Testcontainers](https://testcontainers.com/)** (real SQL Server) + **[Respawn](https://github.com/jbogard/Respawn)**
- Nullable reference types, analyzers, and `TreatWarningsAsErrors` enabled; formatting enforced via `.editorconfig` + `dotnet format`

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for the SQL Server container)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

### Run it

SQL Server must be running before the app or EF migrations will work.

```bash
docker compose up -d                              # SQL Server 2022 on localhost:1433 (from repo root)
dotnet ef database update --project src/realworld-net   # apply migrations
dotnet run --project src/realworld-net            # http://localhost:5268, https://localhost:7215
```

The solution-level commands — `dotnet build`, `dotnet test`, `dotnet format` — run from the repo root. `docker compose` also runs from the root (that's where `compose.yaml` lives), while `dotnet run` and `dotnet ef` target the app project under `src/realworld-net`.

In development, the OpenAPI document is served at `/openapi/v1.json` and the interactive **Scalar UI at `/scalar`** — the easiest way to explore and exercise the endpoints.

### Configuration & secrets

Local settings (connection string, JWT signing key) live in `src/realworld-net/appsettings.Development.json` for convenience. The JWT secret there is a placeholder; for anything beyond local dev, move it out of source with [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets set "JwtSettings:Secret" "<a-long-random-key>" --project src/realworld-net
```

## Running the tests

The suite is **integration tests**: each service is exercised against a real SQL Server instance that [Testcontainers](https://testcontainers.com/) spins up in Docker automatically. **Docker must be running**, but no manual database setup is needed — the tests create, migrate, and tear down their own throwaway container. You do *not* need the `docker compose` SQL Server from above; the tests manage their own.

```bash
dotnet test        # from the repo root — runs the full suite (38 tests)
```

Run a single class or a single test with `--filter`:

```bash
dotnet test --filter "ProfileServiceTests"
dotnet test --filter "FavoriteArticleAsync_Idempotent"
```

Notes:
- All DB tests share **one** SQL Server container for the run and reset state between tests with [Respawn](https://github.com/jbogard/Respawn), so they execute sequentially and in isolation.
- The **first run is slow** — Docker pulls the SQL Server image and boots the container (noticeably slower on Apple Silicon, where the image runs under emulation). Later runs reuse the cached image.

Current coverage: `ArticleServiceTests` (18), `ProfileServiceTests` (10), `UserServiceTests` (10) — covering happy paths, not-found/authorization failures, idempotency (favorite/follow), and concurrency via the DB constraints.

CI runs this same suite (real SQL Server via Testcontainers) on every push and pull request — see the badge at the top and the [workflow](.github/workflows/dotnet-integration-tests.yaml).

## Deployment

Infrastructure is provisioned with **Terraform** (`infra/`), and the app **auto-deploys to Azure App Service** on merge to `main`. The whole stack runs on Azure free tiers.

### Infrastructure (Terraform)

The `infra/` config provisions a resource group, an Azure SQL server + a **serverless free-tier database**, and a **Free (F1) App Service** running the API — with the connection string and JWT secret injected as app settings. The database uses the [`azapi`](https://registry.terraform.io/providers/azure/azapi/latest) provider to enable the SQL free offer (`useFreeLimit`), which the `azurerm` provider doesn't expose.

**Prerequisites:** an Azure subscription, the [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli), and [Terraform](https://developer.hashicorp.com/terraform/install).

```bash
az login

# One-time on a new subscription: register the resource providers this config uses.
az provider register --namespace Microsoft.Sql
az provider register --namespace Microsoft.Web

cd infra
cp terraform.tfvars.example terraform.tfvars   # set sql_admin_password + jwt_secret
terraform init
terraform plan
terraform apply
```

Useful outputs: `web_app_url` (the live site) and `connection_string` (`terraform output -raw connection_string`).

**Region note:** new/free subscriptions can't provision SQL in some regions (`ProvisioningDisabled`). Set `location` in `terraform.tfvars` to one that works for you (e.g. `canadacentral`).

**Teardown** — removes every resource and stops any possibility of billing:

```bash
terraform destroy
```

**Secrets never touch source control.** The SQL password and JWT key are `sensitive` Terraform variables supplied via `terraform.tfvars` or `TF_VAR_*` environment variables, both gitignored — as is the state file (which contains them).

### Continuous deployment

On every **push/merge to `main`**, the [test workflow](.github/workflows/dotnet-integration-tests.yaml) runs; when it succeeds, the [deploy workflow](.github/workflows/deploy.yaml) publishes the app to App Service. The deploy job is **gated on the test run passing** (`workflow_run` + a `conclusion == 'success'` check), so a red build never ships. Manual deploys are available from the Actions tab (`workflow_dispatch`).

EF Core migrations run automatically on app startup, so each deploy provisions/updates the schema with no manual step.

The pipeline needs two repository settings (one-time), both obtained from the Terraform run:

| Kind | Name | Value |
|---|---|---|
| Variable | `AZURE_WEBAPP_NAME` | `terraform output web_app_name` |
| Secret | `AZURE_WEBAPP_PUBLISH_PROFILE` | `az webapp deployment list-publishing-profiles --name <app-name> --resource-group rg-realworld-net --xml` |

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
src/realworld-net/            The API project
  Controllers/   HTTP endpoints (Users, User, Profiles, Articles)
  Services/      Business logic; each service is interface-backed and injected with the DbContext
  Entities/      EF Core-mapped classes (User, Article, Follows, Favorites, Auditable base)
  Models/        API-facing DTOs returned by services (records)
  Dtos/          Request/response payload shapes matching the RealWorld JSON contract
  Middleware/    Global exception handlers (IExceptionHandler)
  Data/          AppDbContext (change-tracked timestamps, relationship config)
  Migrations/    EF Core migrations
tests/realworld-net.Tests/    Test project (xUnit)
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
- End-to-end HTTP tests via `WebApplicationFactory` (the current suite covers the service layer)
