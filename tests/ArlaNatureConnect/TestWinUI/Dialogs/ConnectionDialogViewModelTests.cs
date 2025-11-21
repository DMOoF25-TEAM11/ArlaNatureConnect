using ArlaNatureConnect.WinUI.ViewModels;

namespace ArlaNatureConnect.WinUI.Tests;

[TestClass]
public class ConnectionDialogViewModelTests
{
    [TestMethod]
    public void DefaultConstructor_SetsDefaultConnectionString()
    {
        var vm = new ConnectionDialogViewModel();
        Assert.AreEqual("Integrated Security=True;Encrypt=False;TrustServerCertificate=False;", vm.ConnectionString);
    }

    [TestMethod]
    public void Properties_ComposeConnectionString_WhenIntegratedSecurityFalse()
    {
        var vm = new ConnectionDialogViewModel();
        vm.ServerName = "localhost";
        vm.DatabaseName = "db";
        vm.IntegratedSecurity = false;
        vm.UserName = "sa";
        vm.Password = "p";
        vm.Encrypt = true;
        vm.TrustServerCertificate = true;

        Assert.AreEqual(
            "Server=localhost;Database=db;User ID=sa;Password=p;Encrypt=True;TrustServerCertificate=True;",
            vm.ConnectionString);
    }

    [TestMethod]
    public void SettingConnectionString_ParsesFieldsCorrectly()
    {
        var vm = new ConnectionDialogViewModel();
        vm.ConnectionString = "Data Source=server;Initial Catalog=mydb;User ID=joe;Password=secret;Encrypt=True;TrustServerCertificate=False;";

        Assert.AreEqual("server", vm.ServerName);
        Assert.AreEqual("mydb", vm.DatabaseName);
        Assert.IsFalse(vm.IntegratedSecurity);
        Assert.AreEqual("joe", vm.UserName);
        Assert.AreEqual("secret", vm.Password);
        Assert.IsTrue(vm.Encrypt);
        Assert.IsFalse(vm.TrustServerCertificate);
    }

    [TestMethod]
    public void ResetFields_ClearsFieldsAndSetsDefaults()
    {
        var vm = new ConnectionDialogViewModel();
        vm.ServerName = "x";
        vm.DatabaseName = "y";
        vm.IntegratedSecurity = false;
        vm.UserName = "u";
        vm.Password = "p";
        vm.Encrypt = true;
        vm.TrustServerCertificate = true;

        vm.ResetFields(true);

        Assert.IsNull(vm.ServerName);
        Assert.IsNull(vm.DatabaseName);
        Assert.IsTrue(vm.IntegratedSecurity);
        Assert.IsNull(vm.UserName);
        Assert.IsNull(vm.Password);
        Assert.IsFalse(vm.Encrypt);
        Assert.IsFalse(vm.TrustServerCertificate);
        Assert.AreEqual("Integrated Security=True;Encrypt=False;TrustServerCertificate=False;", vm.ConnectionString);
    }
}
