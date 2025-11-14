# Contributing — C# guidelines, architecture & best practices

This document gives a short, practical set of C# development guidelines, an architectural standard for the repository, and recommended best practices. For naming, formatting and more detailed code-quality rules, see `docs/Iteration-000/QualityCriteriaCode.md`.

## Purpose
- Align team members on architecture and coding habits.
- Improve readability, testability and maintainability.
- Make reviews fast and consistent.

## Architectural standard (recommended)
### Clean / Layered approach:
- `ArlaNatureConnect.Domain` — Entities, domain models, value objects and domain-related attributes (e.g. validation/data-annotation attributes). This project must not reference infrastructure or UI projects.
- `ArlaNatureConnect.Core` — Use-cases, orchestrations and application interfaces; depends only on `Domain`. (Note: because wpf and winui templates include an `Application` namespace this repository uses `Core` for application/use-case logic.)
- `ArlaNatureConnect.Infrastructure` — Persistence, external APIs and concrete implementations of interfaces defined in `Core`.
- `ArlaNatureConnect.[Specific]UI` — Presentation layer (desktop/web/mobile). Replace `[Specific]` with the UI technology, e.g. `WpfUI`, `BlazorUI`, `WinUI`.

### Mvvm patterns for UI projects.
- Mvvm pattern for UI projects: Views, ViewModels, and code-behind only for view-specific logic (e.g. event handlers).
- ViewModels should handle all presentation logic and state.
- Use data binding to connect Views and ViewModels.
- Keep UI projects free of business logic; delegate to `Core` services.
- Use dependency injection to provide services to ViewModels.
- Avoid using `static` members in ViewModels.
- Use commands for user interactions instead of code-behind event handlers where possible.

### Rules
- Directional dependencies only: higher layers can reference lower layers, never reverse.
- Depend on abstractions: define interfaces in `Core` and provide concrete implementations in `Infrastructure`.
- Use constructor injection for dependencies; prefer `IServiceCollection` registration in a single startup/host location.
- Keep domain models free of framework concerns.

### Gotchas
- Avoid static mutable state across layers.

## C# best practices (summary)
- Follow `docs/Iteration-000/QualityCriteriaCode.md` for naming, file layout and encoding.
- Prefer small methods (ideally <30 lines) and single responsibility.
- Use explicit types (per repo policy) and clear names; private fields use underscore prefix: `_myField`.
- Error handling: throw exceptions for exceptional conditions; catch only where you can handle or augment.
- Thread-safety: prefer `Interlocked` or `lock` for simple counters; avoid coarse locks that block async flows.
- Async: use `async/await` end-to-end; avoid `.Result` / `.Wait()` on tasks.
- Tests: add unit tests for business logic and integration tests for external dependencies.
- Performance: profile before optimizing. Use __Test Explorer__ and CI profiling as needed.

### Rules
- Use expression-bodied members for simple getters, setters, and methods.
- Prefer pattern matching and switch expressions for clarity.
- Use explicit type declarations instead of `var`.
- Favor composition over inheritance; prefer interfaces and extension methods.
- Use `using` declarations for disposable resources.
- Prefer `StringBuilder` for complex string manipulations.
- Use `ConfigureAwait(false)` in library code to avoid deadlocks.
- Leverage nullable reference types for better null safety.
- Utilize C# 10+ features like global usings and file-scoped namespaces for cleaner code.
- Adopt records for immutable data structures when appropriate.

## Tooling & editor settings
- Add an `.editorconfig` and enforce with CI.
- Use Visual Studio settings for consistent formatting: __Tools > Options > Text Editor > C# > Formatting__ and __Tools > Options > Text Editor > C# > Code Style__.
- Use __Test Explorer__ and check the __Output__ pane for test logs.

## Testing
- Write unit tests for business logic and integration tests for external dependencies.

- 
## Workflow & PR rules
- Work on feature branches, one logical change per PR.
- Include unit tests and update docs when behavior changes.
- PR checklist: builds clean, tests pass, follows naming & architecture, includes changelog notes when required.

## Rules for team members and Copilot
This repository expects disciplined human review and safe use of AI assistants. Follow these compact rules to keep quality, security and ownership high.

### Team member rules
- Ownership: each PR must have an assigned reviewer and a short description of the goal and scope.
- Branches & commits: use `feature/<ticket>-short-desc`, `fix/<ticket>-short-desc` and clear atomic commits with imperative messages.
- Tests: every behavioral change must include or update unit tests. CI must pass before merging.
- Review checklist: readability, design (layering & abstractions), security (no secrets), performance, tests, and changelog.
- Approvals: at least one approving reviewer and green CI required for merge; larger changes require two reviewers.
- Revert policy: include a clear revert PR if a merge causes regressions; annotate the cause and mitigation.

### Copilot & AI assistant rules
- Treat suggestions as drafts: always review and verify generated code for correctness, security, licensing, and alignment with architecture.
- Minimal prompting: supply focused prompts, show existing function signatures and tests when asking for code.
- Attribution & provenance: add a short comment when a non-trivial block is generated by Copilot, and include a short test that demonstrates expected behavior.
- No secrets: never paste credentials, keys, or PII into AI prompts or generated code.
- License & dependency checks: validate that any suggested libraries are compatible with the repo license and CI policies before adding.
- Testing requirement: any Copilot-generated logic must include unit tests and be reviewed by a human before merging.
- Security review: flag and escalate any suggestions that interact with I/O, network, serialization, or authentication for an extra security review.
- Small PRs: prefer small focused PRs for AI-assisted code to make review easier.

### Example Copilot workflow
- Add an issue or ticket describing the change and include relevant interfaces and failing tests.
- Create a feature branch and add a draft PR.
- Use Copilot locally to generate a candidate implementation; add tests immediately.
- Push and open the PR with a description noting Copilot was used and where. Assign reviewers, run CI, and iterate.

## Reference
See the detailed naming and code rules in `docs/Iteration-000/QualityCriteriaCode.md`. Follow those rules first; this file complements them with architecture and team workflow guidance.
