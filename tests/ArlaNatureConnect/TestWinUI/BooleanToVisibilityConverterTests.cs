using ArlaNatureConnect.WinUI.Converters;

using Microsoft.UI.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Runtime.Versioning;

namespace TestWinUI;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public class BooleanToVisibilityConverterTests
{
    private readonly BooleanToVisibilityConverter _converter = new();

    [TestMethod]
    public void Convert_True_ReturnsVisible()
    {
        object? result = _converter.Convert(true, typeof(Visibility), null!, string.Empty);
        Assert.IsInstanceOfType(result, typeof(Visibility));
        Assert.AreEqual(Visibility.Visible, (Visibility)result);
    }

    [TestMethod]
    public void Convert_False_ReturnsCollapsed()
    {
        object? result = _converter.Convert(false, typeof(Visibility), null!, string.Empty);
        Assert.IsInstanceOfType(result, typeof(Visibility));
        Assert.AreEqual(Visibility.Collapsed, (Visibility)result);
    }

    [TestMethod]
    public void Convert_NonBooleanValue_ReturnsCollapsed()
    {
        object? result = _converter.Convert("not a bool", typeof(Visibility), null!, string.Empty);
        Assert.IsInstanceOfType(result, typeof(Visibility));
        Assert.AreEqual(Visibility.Collapsed, (Visibility)result);
    }

    [TestMethod]
    public void Convert_True_WithParameter_Inverted_ReturnsCollapsed()
    {
        object? result = _converter.Convert(true, typeof(Visibility), new object(), string.Empty);
        Assert.IsInstanceOfType(result, typeof(Visibility));
        Assert.AreEqual(Visibility.Collapsed, (Visibility)result);
    }

    [TestMethod]
    public void Convert_False_WithParameter_Inverted_ReturnsVisible()
    {
        object? result = _converter.Convert(false, typeof(Visibility), new object(), string.Empty);
        Assert.IsInstanceOfType(result, typeof(Visibility));
        Assert.AreEqual(Visibility.Visible, (Visibility)result);
    }

    [TestMethod]
    public void Convert_NullValue_WithParameter_Inverted_ReturnsVisible()
    {
        object? result = _converter.Convert(null!, typeof(Visibility), new object(), string.Empty);
        Assert.IsInstanceOfType(result, typeof(Visibility));
        Assert.AreEqual(Visibility.Visible, (Visibility)result);
    }

    [TestMethod]
    public void ConvertBack_Visible_ReturnsTrue()
    {
        object? result = _converter.ConvertBack(Visibility.Visible, typeof(bool), null!, string.Empty);
        Assert.IsInstanceOfType(result, typeof(bool));
        Assert.IsTrue((bool)result);
    }

    [TestMethod]
    public void ConvertBack_Collapsed_ReturnsFalse()
    {
        object? result = _converter.ConvertBack(Visibility.Collapsed, typeof(bool), null!, string.Empty);
        Assert.IsInstanceOfType(result, typeof(bool));
        Assert.IsFalse((bool)result);
    }

    [TestMethod]
    public void ConvertBack_Visible_WithParameter_Inverted_ReturnsFalse()
    {
        object? result = _converter.ConvertBack(Visibility.Visible, typeof(bool), new object(), string.Empty);
        Assert.IsInstanceOfType(result, typeof(bool));
        Assert.IsFalse((bool)result);
    }

    [TestMethod]
    public void ConvertBack_NonVisibilityValue_ReturnsFalse()
    {
        object? result = _converter.ConvertBack("not a visibility", typeof(bool), null!, string.Empty);
        Assert.IsInstanceOfType(result, typeof(bool));
        Assert.IsFalse((bool)result);
    }
}
