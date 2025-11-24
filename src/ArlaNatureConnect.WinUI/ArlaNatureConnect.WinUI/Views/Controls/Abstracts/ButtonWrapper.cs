using ArlaNatureConnect.WinUI.ViewModels.Abstracts;

using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

/// <inheritdoc/>
public sealed class ButtonWrapper : IButtonWrapper
{
    /// <summary>
    /// The underlying Button control being wrapped.
    /// </summary>
    private readonly Button _btn;


    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonWrapper"/> class.
    /// </summary>
    /// <param name="btn"></param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public ButtonWrapper(Button btn)
    {
        _btn = btn ?? throw new System.ArgumentNullException(nameof(btn));
    }

    /// <inheritdoc/>
    public Button UnderlyingButton => _btn;

    /// <inheritdoc/>
    public object? CommandParameter
    {
        get => _btn.CommandParameter;
        set => _btn.CommandParameter = value;
    }


    /// <inheritdoc/>
    public object? Tag
    {
        get => _btn.Tag;
        set => _btn.Tag = value;
    }
}
