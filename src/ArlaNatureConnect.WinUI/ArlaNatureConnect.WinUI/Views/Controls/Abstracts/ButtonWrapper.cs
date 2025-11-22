using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using Microsoft.UI.Xaml.Controls;

namespace ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

public sealed class ButtonWrapper : IButtonWrapper
{
    private readonly Button _btn;

    public ButtonWrapper(Button btn)
    {
        _btn = btn ?? throw new System.ArgumentNullException(nameof(btn));
    }

    public Button UnderlyingButton => _btn;

    public object? CommandParameter
    {
        get => _btn.CommandParameter;
        set => _btn.CommandParameter = value;
    }

    public object? Tag
    {
        get => _btn.Tag;
        set => _btn.Tag = value;
    }
}
