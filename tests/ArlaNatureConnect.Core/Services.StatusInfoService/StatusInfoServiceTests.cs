namespace Services;

[TestClass]
public sealed class StatusInfoServiceTests
{
    [TestMethod]
    public void BeginLoading_Toggles_IsLoading_And_RaisesEvents()
    {
        var svc = new StatusInfoService();
        int events = 0;
        svc.StatusInfoChanged += (_, _) => events++;

        Assert.IsFalse(svc.IsLoading);
        Assert.AreEqual(0, events);

        var t1 = svc.BeginLoading();
        Assert.IsTrue(svc.IsLoading);
        Assert.AreEqual(1, events);

        var t2 = svc.BeginLoading();
        Assert.IsTrue(svc.IsLoading);
        Assert.AreEqual(1, events, "Second BeginLoading should not raise when already loading.");

        t1.Dispose();
        Assert.IsTrue(svc.IsLoading, "Still loading because one token remains.");
        Assert.AreEqual(1, events);

        t2.Dispose();
        Assert.IsFalse(svc.IsLoading, "No tokens remain so IsLoading should be false.");
        Assert.AreEqual(2, events, "Dispose of last token should raise a change event.");
    }

    [TestMethod]
    public void IsLoading_Setter_Raises_Only_On_Change()
    {
        var svc = new StatusInfoService();
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

    [TestMethod]
    public void HasDbConnection_Raises_Event_On_Change()
    {
        var svc = new StatusInfoService();
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

    [TestMethod]
    public void BeginLoading_Dispose_Is_Idempotent()
    {
        var svc = new StatusInfoService();
        int events = 0;
        svc.StatusInfoChanged += (_, _) => events++;

        var token = svc.BeginLoading();
        Assert.IsTrue(svc.IsLoading);
        Assert.AreEqual(1, events);

        token.Dispose();
        Assert.IsFalse(svc.IsLoading);
        Assert.AreEqual(2, events);

        // disposing again must not change state or raise additional events
        token.Dispose();
        Assert.IsFalse(svc.IsLoading);
        Assert.AreEqual(2, events);
    }
}