# CRUDViewModelBase

## Table of contents

- [Short summary](#short-summary)
- [Source](#source)
- [API surface](#api-surface)
- [Why use it](#why-use-it)
- [Detailed code example](#detailed-code-example)
  - [1) Domain: `Farm` entity](#1-domain-farm-entity)
  - [2) Repository: `IFarmRepository`](#2-repository-ifarmrepository)
  - [3) View-model: `FarmViewModel` (derived)](#3-view-model-farmviewmodel-derived)
  - [4) XAML: Bind commands and entity fields](#4-xaml-bind-commands-and-entity-fields)
  - [5) Usage: navigating / calling commands](#5-usage-navigating--calling-commands)
- [Notes and best practices](#notes-and-best-practices)

## Short summary

`CRUDViewModelBase<TRepos, TEntity>` extends `ListViewModelBase` and provides a standard scaffold for create/read/update/delete workflows for repository-backed entities. It wires commonly used commands (`Add`, `Save`, `Delete`, `Cancel`, `Refresh`) and exposes state flags such as `IsSaving` and `IsEditMode`. Derived classes implement form-specific hooks used by the base for reset, add, save and load operations.

## Source

- Implementation: `ViewModels/Abstracts/CRUDViewModelBase.cs` (see `src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Abstracts/CRUDViewModelBase.cs`)

## API surface

- Inherits `ListViewModelBase<TRepos, TEntity>` with the same generic constraints.
- Commands (properties):
  - `ICommand AddCommand`
  - `ICommand SaveCommand`
  - `ICommand DeleteCommand`
  - `ICommand CancelCommand`
  - `ICommand RefreshCommand`
- Event:
  - `event EventHandler<TEntity?>? EntitySaved` — raised by derived implementations when an entity has been saved.
- State properties:
  - `bool IsSaving` — indicates an ongoing save/delete operation
  - `bool IsEditMode` — true when an existing entity is being edited
  - `bool IsAddMode` — convenience, `!IsEditMode`
- Lifecycle / abstract hooks (must be implemented by derived classes):
  - `Task OnResetFormAsync()`
  - `Task<TEntity> OnAddFormAsync()`
  - `Task OnSaveFormAsync()`
  - `Task OnLoadFormAsync(TEntity entity)`
- Methods:
  - `new Task LoadAsync(Guid id)` — overrides `ListViewModelBase.LoadAsync` to set `IsEditMode` and call `OnLoadFormAsync` when an entity is found.

## Why use it

- Standardizes CRUD command wiring and enable consistent UI state handling across different entity view-models.
- Reduces boilerplate in derived view-models by providing common command setup, can/enable logic and refresh helpers.
- Separates repository operations from view-model concerns via overridable form hooks — keeps code testable.

## Detailed code example

Below is a compact but complete example showing how to implement a concrete repository and a `FarmViewModel` that derives from `CRUDViewModelBase`. The example includes domain, repository interface, view-model implementation and XAML binding snippets.

### 1) Domain: `Farm` entity

This project already contains a `Farm` entity in `src/ArlaNatureConnect.Domain/Entities/Farm.cs`. Example shape:

```csharp
public class Farm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CVR { get; set; } = string.Empty;
    public Guid PersonId { get; set; }
    public Guid AddressId { get; set; }
}
```

### 2) Repository: `IFarmRepository`

```csharp
public interface IFarmRepository : IRepository<Farm>
{
    // IRepository already exposes GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, etc.
    // Add domain-specific queries if needed, e.g.:
    Task<IEnumerable<Farm>> GetByRegionAsync(string region);
}
```

### 3) View-model: `FarmViewModel` (derived)

The view-model implements the abstract form hooks used by `CRUDViewModelBase`. It demonstrates add/save/delete workflows, raising `EntitySaved` when an entity is persisted.

```csharp
public class FarmViewModel : CRUDViewModelBase<IFarmRepository, Farm>
{
    public FarmViewModel(IStatusInfoServices statusInfoServices, IAppMessageService appMessageService, IFarmRepository repository)
        : base(statusInfoServices, appMessageService, repository)
    {
    }

    protected override Task OnResetFormAsync()
    {
        // Reset any temporary form fields or validation state here
        // base.Entity is already set to null by the base OnResetAsync
        return Task.CompletedTask;
    }

    protected override Task<Farm> OnAddFormAsync()
    {
        // Create a new instance from form-bound values (here we rely on Entity as the backing model)
        var newFarm = Entity ?? new Farm { Id = Guid.NewGuid() };

        // You may copy fields from temporary view-model properties into newFarm here
        return Task.FromResult(newFarm);
    }

    protected override async Task OnSaveFormAsync()
    {
        // If Entity is null, treat as add, otherwise update
        if (Entity == null)
            return;

        try
        {
            IsSaving = true;

            if (IsAddMode)
            {
                var toAdd = await OnAddFormAsync();
                await Repository.AddAsync(toAdd);
                base.Entity = toAdd; // set persisted entity
            }
            else
            {
                await Repository.UpdateAsync(Entity);
            }

            // Notify listeners that entity was saved
            EntitySaved?.Invoke(this, base.Entity);
        }
        finally
        {
            IsSaving = false;
        }
    }

    protected override async Task OnLoadFormAsync(Farm entity)
    {
        // Populate any form-specific properties from the loaded entity
        // In this simple example we keep Entity as the form model
        await Task.CompletedTask;
    }
}
```

Notes about the example implementation:
- `OnAddFormAsync` returns a fully prepared `Farm` instance to be persisted by `OnSaveFormAsync` when adding.
- `OnSaveFormAsync` toggles `IsSaving` and calls repository add/update methods. The base class handles command availability via `IsSaving` and `IsEditMode`.
- `EntitySaved` is raised after successfully persisting the entity to allow pages or other components to react.

### 4) XAML: Bind commands and entity fields

```xml
<Page
  x:Class="ArlaNatureConnect.WinUI.Views.FarmPage"
  xmlns:vm="using:ArlaNatureConnect.WinUI.ViewModels">
  <Page.DataContext>
    <!-- Resolve with DI or instantiate for simple demos -->
    <vm:FarmViewModel />
  </Page.DataContext>

  <StackPanel Padding="12">
    <TextBox Text="{Binding Entity.Name, Mode=TwoWay}" PlaceholderText="Farm name" />
    <TextBox Text="{Binding Entity.CVR, Mode=TwoWay}" PlaceholderText="CVR" />

    <!-- PersonId and AddressId often come from lookups; show as read-only for simplicity -->
    <TextBlock Text="{Binding Entity.PersonId}" />
    <TextBlock Text="{Binding Entity.AddressId}" />

    <StackPanel Orientation="Horizontal" Spacing="8">
      <Button Content="Add" Command="{Binding AddCommand}" />
      <Button Content="Save" Command="{Binding SaveCommand}" />
      <Button Content="Delete" Command="{Binding DeleteCommand}" />
      <Button Content="Cancel" Command="{Binding CancelCommand}" />
      <Button Content="Refresh" Command="{Binding RefreshCommand}" />
    </StackPanel>

    <TextBlock Text="{Binding IsSaving}" />
    <TextBlock Text="{Binding IsEditMode}" />
  </StackPanel>
</Page>
```

### 5) Usage: navigating / calling commands

- To edit an existing farm: call `await farmViewModel.LoadAsync(existingId);` — the view-model will set `IsEditMode=true` and `Entity` to the repository result.
- To create a new farm: ensure `IsAddMode` is true (default when no entity loaded). Populate `Entity` fields (or use form-bound temp properties). Then execute `SaveCommand`.
- The base class manages `CanExecute` for commands; call `RefreshCommand` when you need to force re-evaluation.

## Notes and best practices

- Keep long-running repository calls off the UI thread; the base class already operates asynchronously but derived hooks must also avoid blocking work.
- Surface validation errors to `IAppMessageService` so the base class `CanSubmitCore` can incorporate `HasErrorMessages`.
- Prefer simple form models (use `Entity` as the form backing) or create separate DTOs if editing must be transactional and cancelable.
- Unit test derived view-models by mocking `TRepos`, `IStatusInfoServices` and `IAppMessageService` and exercising commands and `LoadAsync`.
