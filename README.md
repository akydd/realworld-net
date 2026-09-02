# realworld-net

An ASP.NET Core (net10.0) implementation of the [RealWorld](https://realworld-docs.netlify.app/) ("Conduit") backend API.

## Getting started

SQL Server must be running before the app or EF migrations will work.

```bash
docker compose up -d       # SQL Server 2022 on localhost:1433
dotnet ef database update  # apply migrations
dotnet run                 # http://localhost:5268, https://localhost:7215
```

In development, the OpenAPI document is at `/openapi/v1.json` and the Scalar API reference UI at `/scalar`.

## Design decisions

### Authentication: hand-rolled `User` table, not `IdentityDbContext`

This project stores users in its own `User` table with a `PasswordHash` column and issues JWTs directly, rather than adopting ASP.NET Core Identity's `IdentityDbContext`. The reasoning:

- **The RealWorld contract doesn't need what Identity provides.** `IdentityDbContext` brings ~7 tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserClaims`, `AspNetUserRoles`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetRoleClaims`). RealWorld has no roles, external logins, claims tables, email confirmation, or lockout — those tables would sit empty.

- **Identity is built around cookie-based sign-in.** Its `UserManager`/`SignInManager` pipeline defaults to cookie auth, whereas RealWorld is a stateless **JWT bearer** API returning a fixed `{ user: { email, token, username, bio, image } }` shape. Using Identity would mean bending it away from its defaults.

- **The entity stays clean.** `IdentityUser` defaults to a string/GUID key and columns we'd never use (`NormalizedUserName`, `SecurityStamp`, `PhoneNumber`, `TwoFactorEnabled`, …). Our `Entities.User` is an `int Id` plus exactly the RealWorld fields.

**What we borrow instead:** only Identity's password hasher. `Microsoft.Extensions.Identity.Core` is referenced so `Services.PasswordHasher` can wrap `PasswordHasher<object>` (PBKDF2, salted, versioned) — the hashing algorithm without the DbContext or schema. JWTs are issued by `Services.JWTService` via `Microsoft.AspNetCore.Authentication.JwtBearer`.

The token returned to clients is **not** a DB column — it's derived per request by signing the user's id. Only the password hash is persisted.
