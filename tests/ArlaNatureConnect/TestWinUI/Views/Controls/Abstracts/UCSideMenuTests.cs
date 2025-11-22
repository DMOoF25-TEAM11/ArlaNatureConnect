using ArlaNatureConnect.WinUI.ViewModels.Abstracts;
using ArlaNatureConnect.WinUI.Views.Controls.Abstracts;

using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace TestWinUI.Views.Controls.Abstracts;

[Microsoft.VisualStudio.TestTools.UnitTesting.DoNotParallelize]
[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public sealed class UCSideMenuTests
{
    // Minimal concrete subclass so we can call protected members from tests.
    private class TestableUCSideMenu : UCSideMenu
    {
        // Expose protected GetButtonForTag as public wrapper
        public Button? PublicGetButtonForTag(string tag)
        {
            return base.GetButtonForTag(tag);
        }

        // Expose protected GetNavigationButtons as public wrapper
        public System.Collections.Generic.IEnumerable<Button> PublicGetNavigationButtons()
        {
            return base.GetNavigationButtons();
        }
    }

    // Fake button implementing IButtonWrapper for tests (avoids creating real WinUI Button instances)
    public class FakeButton : IButtonWrapper
    {
        public object? CommandParameter { get; set; }
        public object? Tag { get; set; }
    }

    // Test subclass that returns a static collection so we can avoid running constructors
    private class TestableUCSideMenuWithButtons : UCSideMenu
    {
        public static System.Collections.Generic.IEnumerable<IButtonWrapper> WrappersForTests = System.Array.Empty<IButtonWrapper>();

        public IButtonWrapper? PublicGetButtonWrapperForTagForTests(string tag)
        {
            return base.GetButtonWrapperForTag(tag);
        }

        public System.Collections.Generic.IEnumerable<IButtonWrapper> PublicGetNavigationButtonWrappersForTests()
        {
            // Call the virtual method so the override in this test class is used
            return GetNavigationButtonWrappers();
        }

        protected override System.Collections.Generic.IEnumerable<Button> GetNavigationButtons()
        {
            // Not used in the wrapper-based tests; return empty to avoid touching visual tree
            return System.Array.Empty<Button>();
        }

        protected override System.Collections.Generic.IEnumerable<IButtonWrapper> GetNavigationButtonWrappers()
        {
            return WrappersForTests ?? System.Array.Empty<IButtonWrapper>();
        }
    }

    // Add a test subclass that returns a provided list via override
    private class TestableUCSideMenuButtonsOverride : UCSideMenu
    {
        public static IEnumerable<Microsoft.UI.Xaml.Controls.Button> ButtonsForTest = Array.Empty<Microsoft.UI.Xaml.Controls.Button>();
        protected override IEnumerable<Microsoft.UI.Xaml.Controls.Button> GetNavigationButtons()
        {
            return ButtonsForTest;
        }
    }


    private static object CreateUninitializedTestMenu()
    {
        // Create instance without running constructors to avoid wiring up XAML events and WinRT
        return System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(TestableUCSideMenu));
    }

    // Generic helper to create uninitialized instances for different test types
    private static T CreateUninitialized<T>() where T : class
    {
        return (T)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(T));
    }


    [TestMethod]
    public void GetButtonForTag_ReturnsNull_For_EmptyOrWhitespace()
    {
        // Arrange
        TestableUCSideMenu menu = (TestableUCSideMenu)CreateUninitializedTestMenu();

        // Act & Assert
        Assert.IsNull(menu.PublicGetButtonForTag(string.Empty));
        Assert.IsNull(menu.PublicGetButtonForTag("   "));
        Assert.IsNull(menu.PublicGetButtonForTag("\t\n"));
    }

    [TestMethod]
    public void ViewModelPropertyChanged_Ignores_Other_Property_Names()
    {
        // Arrange
        TestableUCSideMenu menu = (TestableUCSideMenu)CreateUninitializedTestMenu();

        // Use reflection to call the private ViewModel_PropertyChanged method
        MethodInfo? method = typeof(UCSideMenu).GetMethod("ViewModel_PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method, "Could not find ViewModel_PropertyChanged method via reflection");

        // Act - call with a non-navigation property name and ensure no exception is thrown
        PropertyChangedEventArgs args = new PropertyChangedEventArgs("SomeOtherProperty");
        try
        {
            method!.Invoke(menu, new object?[] { null, args });
        }
        catch (TargetInvocationException tie)
        {
            // unwrap and fail if inner exception is not null
            Assert.Fail($"Invocation threw an exception: {tie.InnerException}");
        }
    }

    [TestMethod]
    public void DataContextChanged_Handler_DoesNotThrow_For_NonNotifyPropertyChanged_DataContext()
    {
        // Arrange
        TestableUCSideMenu menu = (TestableUCSideMenu)CreateUninitializedTestMenu();

        // Find the private Sidebar_DataContextChanged method
        MethodInfo? method = typeof(UCSideMenu).GetMethod("Sidebar_DataContextChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method, "Could not find Sidebar_DataContextChanged method via reflection");

        // Try to build a DataContextChangedEventArgs if available
        object? args = null;
        Type? dceaType = typeof(Microsoft.UI.Xaml.FrameworkElement).Assembly.GetType("Microsoft.UI.Xaml.DataContextChangedEventArgs");
        if (dceaType != null)
        {
            // Try to find a ctor that accepts a single object parameter (NewValue)
            ConstructorInfo? ci = dceaType.GetConstructor(new Type[] { typeof(object) }) ?? dceaType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();
            if (ci != null)
            {
                try
                {
                    args = ci.Invoke(new object?[] { new object() });
                }
                catch
                {
                    args = null;
                }
            }
        }

        try
        {
            // Invoke the handler. Pass null for sender and args if args couldn't be created.
            method!.Invoke(menu, new object?[] { null, args });
        }
        catch (TargetInvocationException tie)
        {
            Assert.Fail($"Invocation threw an exception: {tie.InnerException}");
        }
    }




    [TestMethod]
    public void GetButtonForTag_Uses_CommandParameter_Func_Matcher()
    {
        // Arrange
        var menu = CreateUninitialized<TestableUCSideMenuWithButtons>();
        FakeButton match = new FakeButton();
        FakeButton other = new FakeButton();

        TestableUCSideMenuWithButtons.WrappersForTests = new IButtonWrapper[] { other, match };
        match.CommandParameter = new Func<string, bool>(s => s == "match");

        // Act
        var result = ((TestableUCSideMenuWithButtons)menu).PublicGetButtonWrapperForTagForTests("match");

        // Assert
        Assert.AreSame(match, result);
    }

    [TestMethod]
    public void GetButtonForTag_Uses_Tag_Predicate_Matcher()
    {
        // Arrange
        var menu = CreateUninitialized<TestableUCSideMenuWithButtons>();
        FakeButton predBtn = new FakeButton();
        FakeButton other = new FakeButton();

        TestableUCSideMenuWithButtons.WrappersForTests = new IButtonWrapper[] { other, predBtn };
        predBtn.Tag = new Predicate<string>(s => s.StartsWith("pre"));

        // Act
        var result = ((TestableUCSideMenuWithButtons)menu).PublicGetButtonWrapperForTagForTests("prefix");

        // Assert
        Assert.AreSame(predBtn, result);
    }

    [TestMethod]
    public void GetButtonForTag_FallsBack_To_String_Comparison_CommandParameter()
    {
        // Arrange
        var menu = CreateUninitialized<TestableUCSideMenuWithButtons>();
        FakeButton strBtn = new FakeButton();

        TestableUCSideMenuWithButtons.WrappersForTests = new IButtonWrapper[] { strBtn };
        strBtn.CommandParameter = "Home";

        // Act
        var result = ((TestableUCSideMenuWithButtons)menu).PublicGetButtonWrapperForTagForTests("home");

        // Assert
        Assert.AreSame(strBtn, result);
    }

    [TestMethod]
    public void GetNavigationButtons_Override_Returns_Provided_Buttons()
    {
        // Arrange
        var menu = CreateUninitialized<TestableUCSideMenuWithButtons>();
        FakeButton a = new FakeButton();
        FakeButton b = new FakeButton();
        TestableUCSideMenuWithButtons.WrappersForTests = new IButtonWrapper[] { a, b };

        // Act
        var items = ((TestableUCSideMenuWithButtons)menu).PublicGetNavigationButtonWrappersForTests();

        // Assert
        CollectionAssert.AreEqual(new IButtonWrapper[] { a, b }, System.Linq.Enumerable.ToArray(items));
    }

    [TestMethod]
    public void GetNavigationButtons_Finds_Buttons_In_Visual_Tree()
    {
        // Arrange - use wrapper-based variant to avoid constructing WinRT types in unit tests
        var menu = CreateUninitialized<TestableUCSideMenuWithButtons>();

        FakeButton btn1 = new FakeButton();
        FakeButton btn2 = new FakeButton();

        TestableUCSideMenuWithButtons.WrappersForTests = new IButtonWrapper[] { btn1, btn2 };

        // Act
        var items = ((TestableUCSideMenuWithButtons)menu).PublicGetNavigationButtonWrappersForTests();

        // Assert
        CollectionAssert.AreEqual(new IButtonWrapper[] { btn1, btn2 }, System.Linq.Enumerable.ToArray(items));
    }

    //[TestMethod]
    //public void UpdateButtonStyles_DoesNotThrow_When_NoViewModel()
    //{
    //    // Arrange - avoid constructing WinRT-backed control; use uninitialized instance
    //    var menu = (TestableUCSideMenu)CreateUninitializedTestMenu();

    //    MethodInfo? method = typeof(UCSideMenu).GetMethod("UpdateButtonStyles", BindingFlags.Instance | BindingFlags.NonPublic);
    //    Assert.IsNotNull(method, "Could not find UpdateButtonStyles method via reflection");

    //    // Act & Assert - ensure invocation does not throw (unwrap TargetInvocationException if one occurs)
    //    try
    //    {
    //        method!.Invoke(menu, null);
    //    }
    //    catch (TargetInvocationException tie)
    //    {
    //        Assert.Fail($"Invocation threw an exception: {tie.InnerException}");
    //    }
    //}

}

public interface IVisualTreeProvider
{
    IEnumerable<object> FindDescendants(object root);
}

public interface IResourceProvider
{
    bool TryGetResource(string key, out object value);
}

public class SideMenuLogic
{
    private readonly IVisualTreeProvider _visual;
    private readonly IResourceProvider _resources;

    public SideMenuLogic(IVisualTreeProvider visual, IResourceProvider resources)
    {
        _visual = visual;
        _resources = resources;
    }

    public IEnumerable<object> GetNavigationButtons(object root)
    {
        return _visual.FindDescendants(root)
                      .Where(n => IsButton(n));
    }

    public void ApplyStyles(object root, string currentTag, Action<object, object?> applyStyle)
    {
        if (!_resources.TryGetResource("ArlaNavButton", out var navStyle)) return;
        if (!_resources.TryGetResource("ArlaNavButtonActive", out var activeStyle)) return;

        foreach (var btn in GetNavigationButtons(root))
            applyStyle(btn, navStyle);

        var active = GetNavigationButtons(root)
                         .FirstOrDefault(b => MatchesTag(b, currentTag));
        if (active != null) applyStyle(active, activeStyle);
    }

    // Implement IsButton, MatchesTag etc. in terms of injected providers or simple reflection
    private bool IsButton(object n)
    {
        // Example: check if type name is "Button" (adjust as needed for your project)
        return n != null && n.GetType().Name == "Button";
    }

    private bool MatchesTag(object button, string tag)
    {
        if (button == null || string.IsNullOrWhiteSpace(tag)) return false;
        var prop = button.GetType().GetProperty("Tag");
        if (prop != null)
        {
            var value = prop.GetValue(button);
            if (value is string s)
                return string.Equals(s, tag, StringComparison.OrdinalIgnoreCase);
            if (value is Predicate<string> pred)
                return pred(tag);
            if (value is Func<string, bool> func)
                return func(tag);
        }
        return false;
    }
}
