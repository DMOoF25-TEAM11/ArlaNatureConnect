using ArlaNatureConnect.Core.Services;

namespace TestCore.Services;

[TestClass]
public sealed class AppMessageServiceTests
{
    [TestMethod]
    public void Defaults_AreEmpty()
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
    public void StatusMessages_Set_Get_And_Null_IsIgnored()
    {
        var svc = new AppMessageService();

        svc.StatusMessages = new[] { "one", "two" };
        Assert.IsTrue(svc.HasStatusMessages);
        CollectionAssert.AreEqual(new[] { "one", "two" }, svc.StatusMessages.ToList());

        // setting null should be ignored (setter guards against null)
        svc.StatusMessages = null!;
        CollectionAssert.AreEqual(new[] { "one", "two" }, svc.StatusMessages.ToList());

        // clearing by assigning an empty enumerable should remove messages
        svc.StatusMessages = Enumerable.Empty<string>();
        Assert.IsFalse(svc.HasStatusMessages);
    }

    [TestMethod]
    public void ErrorMessages_Set_Get_And_Null_ResetsToEmpty()
    {
        var svc = new AppMessageService();

        // null should become empty
        svc.ErrorMessages = null!;
        Assert.IsFalse(svc.HasErrorMessages);
        Assert.IsFalse(svc.ErrorMessages.Any());

        svc.ErrorMessages = new List<string> { "err1" };
        Assert.IsTrue(svc.HasErrorMessages);
        CollectionAssert.AreEqual(new[] { "err1" }, svc.ErrorMessages.ToList());

        svc.ErrorMessages = Enumerable.Empty<string>();
        Assert.IsFalse(svc.HasErrorMessages);
    }

    [TestMethod]
    public void EntityName_Set_Get()
    {
        var svc = new AppMessageService();
        Assert.IsNull(svc.EntityName);

        svc.EntityName = "MyEntity";
        Assert.AreEqual("MyEntity", svc.EntityName);
    }
}
