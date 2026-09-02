# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

An ASP.NET Core (net10.0) implementation of the [RealWorld](https://realworld-docs.netlify.app/) ("Conduit") backend API. Early-stage: only user registration is wired end-to-end. The project deliberately uses a hand-rolled `User` table + JWT rather than ASP.NET Core Identity's `IdentityDbContext`; see `README.md` ("Design decisions") for the full rationale.

## Commands

The SQL Server database must be running before the app or migrations will work.

```bash
docker compose up -d          # start SQL Server 2022 on localhost:1433 (sa / P@ssword)
dotnet run                    # run the API (http://localhost:5268, https://localhost:7215)
dotnet build                  # build
dotnet ef migrations add <Name>   # create a migration after changing entities
dotnet ef database update         # apply migrations to the DB
```

API docs (dev only): Scalar UI is served via `app.MapScalarApiReference()` — reachable at `/scalar` alongside the OpenAPI document at `/openapi/v1.json`. There is no test project yet.

## Formatting & linting

Style is codified in `.editorconfig` (file-scoped namespaces, `var` preferences, `I`-prefixed interfaces, `_camelCase` private fields, LF line endings, no BOM).

```bash
dotnet format                     # auto-fix formatting/style across the tree
dotnet format --verify-no-changes # check-only (use in CI); non-zero exit if unformatted
```

The build enforces correctness, not style: `TreatWarningsAsErrors=true` (in the csproj) makes compiler/nullable, security (e.g. `NUxxxx`), and analyzer warnings **fail the build**, while formatting/style rules stay advisory — surfaced by `dotnet format` and the IDE, not the build. So `dotnet build` passing does not mean the code is formatted; run `dotnet format` before committing.

EF's migration scaffolder emits block-scoped namespaces and a BOM, which don't match `.editorconfig`. Run `dotnet format` after `dotnet ef migrations add` to normalize the generated file.

## Architecture

Request flow is a conventional layered pipeline: **Controller → IUserService → AppDbContext (EF Core / SQL Server)**, with two distinct `User` types on either side of the service boundary.

- **Two `User` types — do not conflate them.** `Entities.User` is the EF-mapped DB row (has `Id`, `PasswordHash`, unique indexes on `Username`/`Email`). `Models.User` is the API-facing model returned by services (has `Token`, no password). `UserService` aliases the entity as `DbUser` to disambiguate. When adding endpoints, map entity → model at the service layer and never leak `PasswordHash`.

- **DTO nesting mirrors the RealWorld spec.** Request/response bodies wrap their payload in a `user` object, so DTOs come in pairs: an inner record (`RegisterUserInnerDto`, `UserResponseInnerDto`) plus an outer wrapper (`RegisterUserDto`, `UserResponseDto`). Preserve this nesting for spec compliance.

- **Services are interface-backed and registered scoped** in `Program.cs`: `IUserService`, `IPasswordHasher`, `IJWTService`. `JWTService` signs an HS256 token carrying an `id` claim; `PasswordHasher` wraps Identity's `PasswordHasher<object>`.

- **Error handling is centralized, not per-controller:**
  - **Validation errors** → `Program.cs` overrides `ApiBehaviorOptions.InvalidModelStateResponseFactory` to return `422 Unprocessable Entity` with an `{ "errors": { field: [messages] } }` body (RealWorld's expected shape), instead of the default `400`.
  - **Unique-constraint violations** → `DuplicateExceptionHandler` (an `IExceptionHandler`) catches `UniqueConstraintException` from the `EntityFrameworkCore.Exceptions` package and returns `409 Conflict`. This is why DB inserts don't pre-check for duplicates — the DB unique index + this handler are the mechanism.

## Conventions & known rough edges

- `dotnet ef` reads `appsettings.Development.json` for the connection string; the JWT secret also lives there. These are committed for local dev but should move to user-secrets/env before any real deployment.
- The request path is async end-to-end (`await`ed controller → `await SaveChangesAsync()`); keep new service methods genuinely async rather than blocking with `.Result`/`.Wait()`. `_context.Users.Add` stays synchronous by design — `AddAsync` is only needed for value generators like `HiLo`, not identity keys.
