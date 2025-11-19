# ViewModelBase

## Table of contents

- [Short summary](#short-summary)
- [Source](#source)
- [API surface](#api-surface)
- [Why use it](#why-use-it)
- [Quick examples](#quick-examples)
  - [1) Simple view-model](#1-simple-view-model)
  - [2) Bind in XAML and set DataContext](#2-bind-in-xaml-and-set-datacontext)
- [Related base classes](#related-base-classes)
- [Notes and best practices](#notes-and-best-practices)

## Short summary

`ViewModelBase` is a minimal base class that implements `INotifyPropertyChanged` and exposes a protected `OnPropertyChanged` helper.
Use it to simplify implementing view models for WinUI data binding and to keep notification boilerplate consistent across the app.

## Source

- Implementation: [ViewModelBase.cs](../../../src/ArlaNatureConnect.WinUI/ArlaNatureConnect.WinUI/ViewModels/Abstracts/ViewModelBase.cs)

## API surface

- Implements `INotifyPropertyChanged`
- `event PropertyChangedEventHandler? PropertyChanged`
- `protected void OnPropertyChanged([CallerMemberName] string? name = null)` — raise `PropertyChanged` for `name`

## Why use it

- Centralizes property-change notification logic so individual view models only need to call `OnPropertyChanged()` from setters.
- Keeps view models small and focused on presentation logic.
- Makes it easy to extend common view-model functionality in derived abstract base classes (for example, navigation or CRUD helpers).

## Quick examples

### 1) Simple view-model

```csharp
public class MainViewModel : ViewModelBase
{
    private string? _title;
    public string? Title
    {
        get => _title;
        set
        {
            if (_title == value) return;
            _title = value;
            OnPropertyChanged(); // CallerMemberName will use "Title"
        }
    }

    public MainViewModel()
    {
        Title = "Hello, Arla Nature!";
    }
}
```

### 2) Bind in XAML and set DataContext

XAML (page):

```xml
<Page
  x:Class="ArlaNatureConnect.WinUI.Views.MainPage"
  xmlns:vm="using:ArlaNatureConnect.WinUI.ViewModels"
  Loaded="OnLoaded">
  <Grid>
    <TextBlock Text="{Binding Title}" />
  </Grid>
</Page>
```

Code-behind (simple):

```csharp
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        this.DataContext = new MainViewModel();
    }
}
```

For larger apps prefer constructor injection or a view-model locator instead of creating view models directly in code-behind.

## Related base classes

This project also contains other abstract view-model base classes that extend `ViewModelBase` with additional features:

- `NavigationViewModelBase` — navigation-specific helpers and `NavigationCommand`.
- `CRUDViewModelBase<TRepos, TEntity>` — common CRUD command wiring and helpers for repository-backed view models.

See their implementations under `src/.../ViewModels/Abstracts/`.

## Notes and best practices

- `OnPropertyChanged()` uses `[CallerMemberName]` so call it with no parameter from property setters.
- Keep UI logic in the view (page/control) and presentation/state logic in view-models.
- Use `RelayCommand` (or equivalent) for commands and raise `CanExecuteChanged` when related properties change.
- Keep view-models unit-testable: favor constructor injection for services and repositories.
