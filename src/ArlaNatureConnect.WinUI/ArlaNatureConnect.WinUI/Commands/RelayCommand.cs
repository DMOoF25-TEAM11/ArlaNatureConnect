using System.Windows.Input;

namespace ArlaNatureConnect.WinUI.Commands;

/// <summary>
/// Provides a basic implementation of the <see cref="ICommand"/> interface for relaying command logic.
/// </summary>
public sealed partial class RelayCommand : ICommand
{
    /// <summary>
    /// The action to execute when the command is invoked.   
    /// </summary>
    private readonly Action _execute;

    /// <summary>
    /// The function that determines whether the command can execute. 
    /// </summary>
    private readonly Func<bool>? _canExecute;

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class.
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked.</param>
    /// <param name="canExecute">A function that determines whether the command can execute. If null, the command is always executable.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="execute"/> is null.</exception>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command. This parameter is ignored.</param>
    /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="parameter">Data used by the command. This parameter is ignored.</param>
    public void Execute(object? parameter) => _execute();

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        EventHandler? handler = CanExecuteChanged;
        if (handler == null) return;

        // Invoke each subscriber individually so a throwing handler does not prevent others from running
        foreach (EventHandler single in handler.GetInvocationList())
        {
            try
            {
                single.Invoke(this, EventArgs.Empty);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // Swallow COM exceptions coming from UI-layer handlers to avoid crashing view-model logic
            }
            catch
            {
                // Swallow other exceptions to keep RaiseCanExecuteChanged defensive
            }
        }
    }
}

