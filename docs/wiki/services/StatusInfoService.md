# StatusInfoService

## Table of contents

- [Short summary](#short-summary)
- [Source](#source)
- [API surface](#api-surface)
- [Why use it](#why-use-it)
- [Registration](#registration)
- [Quick usage patterns](#quick-usage-patterns)
  - [1) Bind a `StatusBar` (or similar control) to the service](#1-bind-a-statusbar-or-similar-control-to-the-service)
  - [2) Use `BeginLoading()` in a ViewModel while loading data](#2-use-beginloading-in-a-viewmodel-while-loading-data)
- [Notes and best practices](#notes-and-best-practices)
- [Tests](#tests)
- [Class diagram](#class-diagram)

## Short summary

`StatusInfoService` centralizes app-wide status reporting for UI concerns such as "busy/loading" and database connectivity. It provides a single, reliable source of truth so multiple components can signal the same global state without causing UI flicker or race conditions.

## Source

- Implementation: [StatusInfoService.cs](../../../src/ArlaNatureConnect.Core/Services/StatusInfoService.cs)
- Interface: [IStatusInfoServices.cs](../../../src/ArlaNatureConnect.Core/Services/IStatusInfoServices.cs)

## API surface

- `bool IsLoading { get; set; }` — true when one or more callers have signaled loading
- `bool HasDbConnection { get; set; }` — indicates database connectivity
- `IDisposable BeginLoading()` — returns a token; disposing it decrements the loading count
- `event EventHandler? StatusInfoChanged` — raised when any reported status changes

## Why use it

Using a centralized `StatusInfoService` prevents inconsistent UI states when multiple parts of the app perform work concurrently. `BeginLoading()` implements reference-counted loading tokens so `IsLoading` is only false when all callers have finished.

## Registration

```csharp
// Startup / App.xaml.cs
services.AddSingleton<IStatusInfoServices, StatusInfoService>();
```

## Quick usage patterns

### 1) Bind a `StatusBar` (or similar control) to the service

```xml
<!-- StatusBarControl.xaml -->
<UserControl
  x:Class="ArlaNatureConnect.WinUI.Controls.StatusBarControl"
  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
  <StackPanel Orientation="Horizontal" VerticalAlignment="Center" Padding="8">
    <ProgressRing IsActive="{x:Bind Service.IsLoading, Mode=OneWay}" Width="20" Height="20" />
    <TextBlock Margin="8,0,0,0" Text="{x:Bind Service.HasDbConnection ? \"DB: OK\" : \"DB: Offline\", Mode=OneWay}" />
  </StackPanel>
</UserControl>
```

```csharp
// StatusBarControl.xaml.cs
public sealed partial class StatusBarControl : UserControl
{
    public IStatusInfoServices Service { get; }

    public StatusBarControl(IStatusInfoServices service)
    {
        InitializeComponent();
        Service = service;

        // Refresh x:Bind targets when service signals changes
        Service.StatusInfoChanged += (_, __) => DispatcherQueue.TryEnqueue(() => this.Bindings.Update());
    }
}
```

### 2) Use `BeginLoading()` in a ViewModel while loading data

```csharp
public class ProjectsViewModel : ViewModelBase
{
    private readonly IStatusInfoServices _statusInfo;
    private readonly IProjectRepository _repo;

    public IEnumerable<Project>? Projects { get; private set; }

    public ProjectsViewModel(IStatusInfoServices statusInfo, IProjectRepository repo)
    {
        _statusInfo = statusInfo;
        _repo = repo;
    }

    public async Task LoadAsync()
    {
        // Acquire a loading token; IsLoading will be true until the token is disposed
        using (_statusInfo.BeginLoading())
        {
            Projects = await _repo.GetAllAsync();
        }
    }
}
```

## Notes and best practices

- Prefer `BeginLoading()` tokens (via `using`) instead of toggling `IsLoading` directly; the token approach handles concurrent loaders correctly.
- Bind UI elements (ProgressRing, status text) to `IsLoading`/`HasDbConnection` and refresh `x:Bind` with `Bindings.Update()` when `StatusInfoChanged` fires.
- The service swallows exceptions thrown by subscribers to avoid breaking publishers.

## Tests

Unit tests for `StatusInfoService` are located under `tests/ArlaNatureConnect/TestCore/Services/StatusInfoServiceTests.cs` and cover token behavior, event raising, and idempotent disposal.

## Class diagram

```mermaid
classDiagram
    class StatusInfoService {
        +bool IsLoading
        +bool HasDbConnection
        +IDisposable BeginLoading()
        +event EventHandler? StatusInfoChanged
        -int _loadingCount
    }

    interface IStatusInfoServices
    class LoadingToken {
        -StatusInfoService _owner
        +void Dispose()
    }

    interface IConnectionStringService

    StatusInfoService ..|> IStatusInfoServices
    StatusInfoService --> LoadingToken : returns
    LoadingToken --> StatusInfoService : releases on Dispose
