# Solution Dependency Graph

The following Mermaid diagram shows project-to-project references in this solution.

```mermaid
graph TD
  ArlaDomain["ArlaNatureConnect.Domain"]
  ArlaCore["ArlaNatureConnect.Core"]
  ArlaInfra["ArlaNatureConnect.Infrastructure"]
  ArlaWinUI["ArlaNatureConnect.WinUI"]

  ArlaCore --> ArlaDomain
  ArlaInfra --> ArlaCore
  ArlaWinUI --> ArlaInfra
```

Notes:
- Arrows point from the referencing project to the referenced project.
- Put this file in `docs/` so it renders on GitHub with Mermaid support (if enabled) or use a Mermaid renderer when viewing locally.
