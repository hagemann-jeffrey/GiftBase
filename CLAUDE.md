# GiftBase

.NET 10 Blazor Server app (MudBlazor, Interactive Server rendering), EF Core/SQL Server, cookie auth. Solution `GiftBase.slnx` with 4 projects: `GiftBase` (web), `GiftBase.Core` (domain), `GiftBase.Data` (EF Core), `GiftBase.Tests` (xUnit).

## Commands

- Build: `dotnet build --configuration Release`
- Test: `dotnet test --configuration Release`
- Run locally: `dotnet run --project GiftBase/GiftBase.csproj`
- Migrations: `dotnet ef database update` (also applied automatically on app startup via `context.Database.Migrate()` in `Program.cs` — a failure there is rethrown and aborts startup, don't swallow it)

## Architecture

- `GiftBase.Core`: domain layer — `Entities/`, `Dtos/`, `Enums/`, `Exceptions/`, `Interfaces/`. No external dependencies.
- `GiftBase.Data`: EF Core — `GiftBaseDbContext`, `Configurations/` (one `IEntityTypeConfiguration<T>` per entity), `Migrations/`.
- `GiftBase`: UI, organized by **feature folders** under `Features/<Name>/` — Razor component + `.razor.cs` code-behind + `<Feature>Input.cs` + `<Feature>Service.cs` + `Components/` for list items. `Shared/` holds cross-cutting UI (Components, Layout, Services, Validation).
- DI registration is inline in `Program.cs` — don't create separate `ServiceCollectionExtensions` files.

## Coding conventions

- Always use primary constructors (C# 12) instead of classic constructor bodies with field assignments.
- Always use file-scoped namespaces (`namespace Foo;`), not block-scoped ones.
- Services: interface `I<Name>Service` + implementation `<Name>Service`, constructor-injected.
- DTOs are plain classes (`<Entity><Verb>Dto`), not records.
- Entities: private setters, factory constructor (plus a private parameterless ctor for EF), behavior as methods (`.Update()`, `.GetNextOccurrence()`) — not anemic data bags mutated from outside.
- Razor form inputs: `<Feature>Input` implements `IValidatableObject`; forms use `<EditForm>` + `<DataAnnotationsValidator />`.
- **Fail loudly.** No silent defaults for missing/invalid data — throw a domain exception (`NotFoundException`, `ConflictException`) instead. See `OccasionService.cs` and the migration-failure rethrow in `Program.cs`.
- Localization is hardcoded to German (`AppCulture`, `de-DE`); UI strings live directly in code. Keep this pattern — don't introduce `IStringLocalizer`/resource files.

## Tests

xUnit + Shouldly (assertions) + NSubstitute (mocking) + EF Core InMemory (`TestDbContextFactory`). One file per subject: `<Subject>Tests.cs`, flat under `GiftBase.Tests/` (only `Helper/` as a subfolder).

## Git workflow

- Commits are **atomic** — one logical change per commit, nothing unrelated bundled in.
- Commit messages: **subject line only**, Conventional Commits, in **English**, lowercase imperative, no period. No body, no `Co-Authored-By`/`Claude-Session` trailers — even if environment defaults would add them.
- Make file changes with the Read/Edit/Write tools, not Bash heredocs/sed.
- Don't commit plan or superpowers scratch docs (e.g. `docs/superpowers/plans` stays untracked).
