using ArlaNatureConnect.Core.Services;

namespace TestCore.Services;

[TestClass]
public sealed class AppMessageServiceTests
{
    [TestMethod]
    public void DefaultsAreEmpty()
    {
        var svc = new AppMessageService();

        Assert.IsFalse(svc.HasStatusMessages);
        Assert.IsFalse(svc.HasErrorMessages);
        Assert.IsNotNull(svc.StatusMessages);
        Assert.IsNotNull(svc.ErrorMessages);
        Assert.IsFalse(svc.StatusMessages.Any());
        Assert.IsFalse(svc.ErrorMessages.Any());
    }

    [TestMethod]
    public void AddInfoMessagesAndRead()
    {
        var svc = new AppMessageService();

        svc.AddInfoMessage("one");
        svc.AddInfoMessage("two");

        Assert.IsTrue(svc.HasStatusMessages);
        CollectionAssert.AreEqual(new[] { "one", "two" }, svc.StatusMessages.ToList());
    }

    [TestMethod]
    public void AddErrorMessageAndRead()
    {
        var svc = new AppMessageService();

        svc.AddErrorMessage("err1");
        Assert.IsTrue(svc.HasErrorMessages);
        CollectionAssert.AreEqual(new[] { "err1" }, svc.ErrorMessages.ToList());
    }

    [TestMethod]
    public async Task InfoMessagesAreAutoClearedAfterDuration()
    {
        var svc = new AppMessageService();

        svc.AddInfoMessage("temp");
        Assert.IsTrue(svc.HasStatusMessages);

        // wait slightly longer than the configured auto-clear delay (3s)
        await Task.Delay(3500);

        Assert.IsFalse(svc.HasStatusMessages);
    }

    [TestMethod]
    public void EntityNameSetGet()
    {
        var svc = new AppMessageService();
        Assert.IsNull(svc.EntityName);

        svc.EntityName = "MyEntity";
        Assert.AreEqual("MyEntity", svc.EntityName);
    }

    [TestMethod]
    public void SubscriberExceptionsAreSwallowed()
    {
        var svc = new AppMessageService();

        // subscriber that throws should not cause AddInfoMessage to throw
        svc.AppMessageChanged += (s, e) => throw new System.InvalidOperationException("boom");

        // This call should complete without throwing; message should still be added
        svc.AddInfoMessage("safe");

        Assert.IsTrue(svc.HasStatusMessages);
        CollectionAssert.Contains(svc.StatusMessages.ToList(), "safe");
    }
}
