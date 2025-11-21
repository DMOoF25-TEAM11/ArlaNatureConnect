# AppMessageService

## Table of contents

- [Short summary](#short-summary)
- [Source](#source)
- [API surface](#api-surface)
- [Why use it](#why-use-it)
- [Registration](#registration)
- [Quick usage patterns](#quick-usage-patterns)
  - [1) Publish a message from a page or view-model](#1-publish-a-message-from-a-page-or-view-model)
  - [2) Subscribe from a page and update local UI](#2-subscribe-from-a-page-and-update-local-ui)
- [Notes and best practices](#notes-and-best-practices)
- [Tests](#tests)
- [Class diagram](#class-diagram)

## Short summary

`AppMessageService` provides application-level informational and error messages that pages and controls can publish. It is intentionally UI-agnostic — pages host their own UI elements (TextBlock, MessageBanner, Flyout, etc.) and subscribe to the service to display messages.

## Source

- Implementation: [AppMessageService.cs](../../../src/ArlaNatureConnect.Core/Services/AppMessageService.cs)
- Interface: [IAppMessageService.cs](../../../src/ArlaNatureConnect.Core/Services/IAppMessageService.cs)

## API surface

- `AddInfoMessage(string)` — add a short-lived informational message (auto-cleared)
- `AddErrorMessage(string)` — add a persistent error message
- `ClearErrorMessages()` — clear persistent errors
- `StatusMessages`, `ErrorMessages` — enumerables of current messages
- `AppMessageChanged` — event raised when messages change
- `EntityName` — optional placeholder used when messages contain `{EntityName}`

## Why use it

Centralizes how UI components publish notifications without coupling to a particular status control. Each page/control decides how to present messages (banner, toast, inline text) and updates when the service signals changes.

## Registration

```csharp
// Startup / App.xaml.cs
services.AddSingleton<IAppMessageService, AppMessageService>();
```

## Quick usage patterns

### 1) Publish a message from a page or view-model

```csharp
// inside a page or view-model with IAppMessageService injected
_messageService.EntityName = "Projekt"; // optional
_messageService.AddInfoMessage("{EntityName} er gemt.");
```

### 2) Subscribe from a page and update local UI

```csharp
public sealed partial class EditEntityPage : Page
{
    private readonly IAppMessageService _messageService;

    public EditEntityPage(IAppMessageService messageService)
    {
        InitializeComponent();
        _messageService = messageService;
        _messageService.AppMessageChanged += MessageService_AppMessageChanged;
    }

    private void MessageService_AppMessageChanged(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            // Example: update a named XAML element "PageStatusTextBlock"
            PageStatusTextBlock.Text = _messageService.StatusMessages.FirstOrDefault()
                                         ?? _messageService.ErrorMessages.FirstOrDefault()
                                         ?? string.Empty;
        });
    }
}
```

## Notes and best practices

- The service auto-clears info messages after a short duration; error messages remain until `ClearErrorMessages()` is called.
- Prefer constructor injection for pages and view-models to get `IAppMessageService`.
- Keep presentation logic in the page/control — the service only stores messages and raises change notifications.
- Subscribers should swallow exceptions; the service does so already when raising `AppMessageChanged`.

## Tests

There are unit tests for `AppMessageService` under `tests/ArlaNatureConnect/TestCore` that validate default state, adding/clearing messages, auto-clear behavior, and that subscriber exceptions are swallowed.

## Class diagram

```mermaid
classDiagram
    class AppMessageService {
        +void AddInfoMessage(string message)
        +void AddErrorMessage(string message)
        +void ClearErrorMessages()
        +IEnumerable<string> StatusMessages
        +IEnumerable<string> ErrorMessages
        +string? EntityName
        +event EventHandler? AppMessageChanged
        -TimeSpan _infoLifetime
        -System.Threading.Timer? _clearTimer
        -List<string> _statusMessages
        -List<string> _errorMessages
    }

    interface IAppMessageService

    class TimerTask {
        <<utility>>
    }

    class Subscriber {
        <<actor>>
    }

    AppMessageService ..|> IAppMessageService
    AppMessageService --> TimerTask : auto_clear
    AppMessageService --> Subscriber : notifies
