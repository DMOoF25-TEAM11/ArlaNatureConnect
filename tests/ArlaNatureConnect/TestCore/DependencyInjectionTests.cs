using ArlaNatureConnect.Core;
using ArlaNatureConnect.Core.Services;

using Microsoft.Extensions.DependencyInjection;

namespace TestCore;

[TestClass]
public class DependencyInjectionTests
{
    [TestMethod]
    public void Add_Core_Services_Registers_Expected_Services()
    {
        var services = new ServiceCollection();

        // call the extension that registers core services
        services.AddCoreServices();

        using ServiceProvider provider = services.BuildServiceProvider();

        // IStatusInfoServices is registered as Singleton
        var statusA = provider.GetRequiredService<IStatusInfoServices>();
        var statusB = provider.GetRequiredService<IStatusInfoServices>();
        Assert.IsNotNull(statusA);
        Assert.AreSame(statusA, statusB, "IStatusInfoServices should be registered as singleton.");

        // IAppMessageService is registered as Transient
        var msgA = provider.GetRequiredService<IAppMessageService>();
        var msgB = provider.GetRequiredService<IAppMessageService>();
        Assert.IsNotNull(msgA);
        Assert.IsNotNull(msgB);
        Assert.AreNotSame(msgA, msgB, "IAppMessageService should be registered as transient.");
    }


}

