using ArlaNatureConnect.Core.Services;

namespace TestCore.Services;

[TestClass]
public sealed class AppMessageServiceTests
{
    [TestMethod]
    public void Defaults_Are_Empty()
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
    public void Add_Info_Messages_And_Read()
    {
        var svc = new AppMessageService();

        svc.AddInfoMessage("one");
        svc.AddInfoMessage("two");

        Assert.IsTrue(svc.HasStatusMessages);
        CollectionAssert.AreEqual(new[] { "one", "two" }, svc.StatusMessages.ToList());
    }

    [TestMethod]
    public void Add_Error_Message_And_Read()
    {
        var svc = new AppMessageService();

        svc.AddErrorMessage("err1");
        Assert.IsTrue(svc.HasErrorMessages);
        CollectionAssert.AreEqual(new[] { "err1" }, svc.ErrorMessages.ToList());
    }

    [TestMethod]
    public async Task Info_Messages_Are_Auto_Cleared_After_Duration()
    {
        var svc = new AppMessageService();

        svc.AddInfoMessage("temp");
        Assert.IsTrue(svc.HasStatusMessages);

        // wait slightly longer than the configured auto-clear delay (3s)
        await Task.Delay(3500);

        Assert.IsFalse(svc.HasStatusMessages);
    }

    [TestMethod]
    public void EntityName_Set_Get()
    {
        var svc = new AppMessageService();
        Assert.IsNull(svc.EntityName);

        svc.EntityName = "MyEntity";
        Assert.AreEqual("MyEntity", svc.EntityName);
    }

    [TestMethod]
    public void Subscriber_Exceptions_Are_Swallowed()
    {
        var svc = new AppMessageService();

        // subscriber that throws should not cause AddInfoMessage to throw
        svc.AppMessageChanged += (s, e) => throw new System.InvalidOperationException("boom");

        // This call should complete without throwing; message should still be added
        svc.AddInfoMessage("safe");

        Assert.IsTrue(svc.HasStatusMessages);
        CollectionAssert.Contains(svc.StatusMessages.ToList(), "safe");
    }

    [TestMethod]
    public void Clear_Error_Messages_Removes_All_Errors_And_Raises_Event()
    {
        var svc = new AppMessageService();
        bool eventRaised = false;
        svc.AppMessageChanged += (s, e) => eventRaised = true;

        svc.AddErrorMessage("err1");
        svc.AddErrorMessage("err2");

        Assert.IsTrue(svc.HasErrorMessages);
        CollectionAssert.AreEqual(new[] { "err1", "err2" }, svc.ErrorMessages.ToList());

        svc.ClearErrorMessages();

        Assert.IsFalse(svc.HasErrorMessages);
        Assert.IsNotNull(svc.ErrorMessages);
        Assert.IsFalse(svc.ErrorMessages.Any());
        Assert.IsTrue(eventRaised, "ClearErrorMessages should raise AppMessageChanged.");
    }
}
