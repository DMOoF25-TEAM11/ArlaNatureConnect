using ArlaNatureConnect.Core.Services; // Add this at the top if StatusInfoService is in this namespace
using System.Reflection;

namespace TestCore.Services;

[TestClass]
public sealed class StatusInfoServiceTests
{
    private static int GetLoadingCount(StatusInfoService svc)
    {
        FieldInfo? f = typeof(StatusInfoService).GetField("_loadingCount", BindingFlags.Instance | BindingFlags.NonPublic);
        return f is null ? -1 : (int)f.GetValue(svc)!;
    }

    // Verifies that BeginLoading toggles IsLoading correctly and raises events
    [TestMethod]
    public void BeginLoading_Toggles_IsLoading_And_RaisesEvents()
    {
        StatusInfoService svc = new StatusInfoService();
        int events = 0;
        svc.StatusInfoChanged += (_, _) => events++;

        Assert.IsFalse(svc.IsLoading);
        Assert.AreEqual(0, events);
        Assert.AreEqual(0, GetLoadingCount(svc));

        IDisposable t1 = svc.BeginLoading();
        Assert.IsTrue(svc.IsLoading);
        Assert.AreEqual(1, events);
        Assert.AreEqual(1, GetLoadingCount(svc), "_loadingCount should be 1 after first BeginLoading");

        IDisposable t2 = svc.BeginLoading();
        Assert.IsTrue(svc.IsLoading);
        Assert.AreEqual(1, events, "Second BeginLoading should not raise when already loading.");
        Assert.AreEqual(2, GetLoadingCount(svc), "_loadingCount should be 2 after second BeginLoading");

        t1.Dispose();
        Assert.IsTrue(svc.IsLoading, "Still loading because one token remains.");
        Assert.AreEqual(1, events);
        Assert.AreEqual(1, GetLoadingCount(svc), "_loadingCount should decrement to 1 after disposing one token");

        t2.Dispose();
        Assert.IsFalse(svc.IsLoading, "No tokens remain so IsLoading should be false.");
        Assert.AreEqual(2, events, "Dispose of last token should raise a change event.");
        Assert.AreEqual(0, GetLoadingCount(svc), "_loadingCount should be 0 after disposing all tokens");
    }

    // Verifies that setting IsLoading raises events only on actual changes
    [TestMethod]
    public void IsLoading_Setter_Raises_Only_On_Change()
    {
        StatusInfoService svc = new StatusInfoService();
        int events = 0;
        svc.StatusInfoChanged += (_, _) => events++;

        svc.IsLoading = true;
        Assert.IsTrue(svc.IsLoading);
        Assert.AreEqual(1, events);

        svc.IsLoading = true; // no change
        Assert.AreEqual(1, events);

        svc.IsLoading = false;
        Assert.IsFalse(svc.IsLoading);
        Assert.AreEqual(2, events);
    }

    // Verifies that setting HasDbConnection raises events only on actual changes
    [TestMethod]
    public void HasDbConnection_Raises_Event_On_Change()
    {
        StatusInfoService svc = new StatusInfoService();
        int events = 0;
        svc.StatusInfoChanged += (_, _) => events++;

        Assert.IsFalse(svc.HasDbConnection);
        svc.HasDbConnection = true;
        Assert.IsTrue(svc.HasDbConnection);
        Assert.AreEqual(1, events);

        svc.HasDbConnection = true; // no change
        Assert.AreEqual(1, events);

        svc.HasDbConnection = false;
        Assert.IsFalse(svc.HasDbConnection);
        Assert.AreEqual(2, events);
    }

    // Verifies that disposing the loading token multiple times does not affect state after the first dispose
    [TestMethod]
    public void BeginLoading_Dispose_Is_Idempotent()
    {
        StatusInfoService svc = new StatusInfoService();
        int events = 0;
        svc.StatusInfoChanged += (_, _) => events++;

        IDisposable token = svc.BeginLoading();
        Assert.IsTrue(svc.IsLoading);
        Assert.AreEqual(1, events);
        Assert.AreEqual(1, GetLoadingCount(svc));

        token.Dispose();
        Assert.IsFalse(svc.IsLoading);
        Assert.AreEqual(2, events);
        Assert.AreEqual(0, GetLoadingCount(svc));

        // disposing again must not change state or raise additional events
        token.Dispose();
        Assert.IsFalse(svc.IsLoading);
        Assert.AreEqual(2, events);
        Assert.AreEqual(0, GetLoadingCount(svc));
    }
}
