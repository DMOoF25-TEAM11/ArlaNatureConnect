# Contributing — C# guidelines, architecture & best practices

This document gives a short, practical set of C# development guidelines, an architectural standard for the repository, and recommended best practices. For naming, formatting and more detailed code-quality rules, see `docs/Iteration-000/QualityCriteriaCode.md`.

## Purpose
- Align team members on architecture and coding habits.
- Improve readability, testability and maintainability.
- Make reviews fast and consistent.

## Architectural standard (recommended)
Follow a Clean / Layered approach:
- `ArlaNatureConnect.Domain` — Entities, domain models, value objects and domain-related attributes (e.g. validation/data-annotation attributes). This project must not reference infrastructure or UI projects.
- `ArlaNatureConnect.Core` — Use-cases, orchestrations and application interfaces; depends only on `Domain`. (Note: because wpf and winui templates include an `Application` namespace this repository uses `Core` for application/use-case logic.)
- `ArlaNatureConnect.Infrastructure` — Persistence, external APIs and concrete implementations of interfaces defined in `Core`.
- `ArlaNatureConnect.[Specific]UI` — Presentation layer (desktop/web/mobile). Replace `[Specific]` with the UI technology, e.g. `WpfUI`, `BlazorUI`, `WinUI`.

- Rules:
- Directional dependencies only: higher layers can reference lower layers, never reverse.
- Depend on abstractions: define interfaces in `Core` and provide concrete implementations in `Infrastructure`.
- Use constructor injection for dependencies; prefer `IServiceCollection` registration in a single startup/host location.
- Keep domain models free of framework concerns.

Gotchas:
- Avoid static mutable state across layers.
- Keep DB entities and domain models separated (mapping layer).

## C# best practices (summary)
- Follow `docs/Iteration-000/QualityCriteriaCode.md` for naming, file layout and encoding.
- Prefer small methods (ideally <30 lines) and single responsibility.
- Use explicit types (per repo policy) and clear names; private fields use underscore prefix: `_myField`.
- Error handling: throw exceptions for exceptional conditions; catch only where you can handle or augment.
- Thread-safety: prefer `Interlocked` or `lock` for simple counters; avoid coarse locks that block async flows.
- Async: use `async/await` end-to-end; avoid `.Result` / `.Wait()` on tasks.
- Tests: add unit tests for business logic and integration tests for external dependencies.
- Performance: profile before optimizing. Use __Test Explorer__ and CI profiling as needed.

## Tooling & editor settings
- Add an `.editorconfig` and enforce with CI.
- Use Visual Studio settings for consistent formatting: __Tools > Options > Text Editor > C# > Formatting__ and __Tools > Options > Text Editor > C# > Code Style__.
- Use __Test Explorer__ and check the __Output__ pane for test logs.

## Workflow & PR rules
- Work on feature branches, one logical change per PR.
- Include unit tests and update docs when behavior changes.
- PR checklist: builds clean, tests pass, follows naming & architecture, includes changelog notes when required.

## Reference
See the detailed naming and code rules in `docs/Iteration-000/QualityCriteriaCode.md`. Follow those rules first; this file complements them with architecture and team workflow guidance.
