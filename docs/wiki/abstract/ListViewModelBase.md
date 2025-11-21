# ListViewModelBase

## Table of contents

- [Short summary](#short-summary)
- [Source](#source)
- [API surface](#api-surface)
- [Why use it](#why-use-it)
- [Quick examples](#quick-examples)
  - [1) Simple derived list view-model](#1-simple-derived-list-view-model)
  - [2) Load an entity by id](#2-load-an-entity-by-id)
- [Notes and best practices](#notes-and-best-practices)

## Short summary

`ListViewModelBase<TRepos, TEntity>` is an abstract base class for view-models that present or edit a single entity retrieved from a repository.
It centralizes common list/view logic such as holding the current `Entity`, exposing the underlying `Repository`, and providing an asynchronous `LoadAsync(Guid)` pattern that reports loading status and surfaces errors through application services.

## Source

- Implementation: `ViewModels/Abstracts/ListViewModelBase.cs` (see `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Abstracts/ListViewModelBase.cs`)

## API surface

- Generic parameters:
  - `TRepos` — repository type, must implement `IRepository<TEntity>`
  - `TEntity` — entity type (reference type)
- Constructor dependencies:
  - `IStatusInfoServices statusInfoServices` — reports loading/connectivity state
  - `IAppMessageService appMessageService` — surfaces messages and errors to UI
  - `TRepos repository` — repository used to load entities
- Properties:
  - `TEntity? Entity` — currently loaded entity (get/set). Setting raises `OnPropertyChanged`.
  - `TRepos Repository` — repository instance used by the view-model.
- Methods:
  - `Task LoadAsync(Guid id)` — asynchronously loads the entity with the provided id. Uses `BeginLoading()` from `IStatusInfoServices` to report load state and reports errors to `IAppMessageService` instead of throwing.

## Why use it

- Reduces duplication for view-models that rely on repository-backed entities.
- Standardizes load behavior, including UI loading state and error reporting.
- Keeps view-models focused on presentation concerns while delegating data access to repositories and status/messages to services.

## Quick examples

### 1) Simple derived list view-model

```csharp
public class FarmerViewModel : ListViewModelBase<IFarmerRepository, Farmer>
{
    public FarmerViewModel(IStatusInfoServices statusInfoServices, IAppMessageService appMessageService, IFarmerRepository repository)
        : base(statusInfoServices, appMessageService, repository)
    {
    }

    // Additional properties and commands specific to Farmer view
}
```

### 2) Load an entity by id

```csharp
// In a page or when navigating to the view:
await farmerViewModel.LoadAsync(farmerId);

// After the call, farmerViewModel.Entity will be populated (or null if not found).
// Any errors are reported via the configured IAppMessageService; loading UI is handled by IStatusInfoServices.
```

## Notes and best practices

- `LoadAsync` swallows exceptions and reports them via `IAppMessageService.AddErrorMessage` — this keeps UI flows predictable; handle specific error cases in derived view-models if needed.
- Prefer constructor injection for repositories and services to keep view-models testable.
- Use `OnPropertyChanged()` in derived properties to keep bindings up to date.
