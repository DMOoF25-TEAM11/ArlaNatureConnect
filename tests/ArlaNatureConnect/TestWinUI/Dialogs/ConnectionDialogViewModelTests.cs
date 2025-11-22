using ArlaNatureConnect.WinUI.ViewModels;
using Microsoft.Data.SqlClient;

using System.Runtime.Versioning;

namespace ArlaNatureConnect.WinUI.Tests;

[TestClass]
[SupportedOSPlatform("windows10.0.22621.0")]
public class ConnectionDialogViewModelTests
{
    [TestMethod]
    public void DefaultConstructor_SetsDefaultConnectionString()
    {
        ConnectionDialogViewModel vm = new ConnectionDialogViewModel();
        Assert.AreEqual("Integrated Security=True;Encrypt=False;TrustServerCertificate=False;", vm.ConnectionString);
    }

    [TestMethod]
    public void Properties_ComposeConnectionString_WhenIntegratedSecurityFalse()
    {
        ConnectionDialogViewModel vm = new ConnectionDialogViewModel();
        vm.ServerName = "localhost";
        vm.DatabaseName = "db";
        vm.IntegratedSecurity = false;
        vm.UserName = "sa";
        vm.Password = "p";
        vm.Encrypt = true;
        vm.TrustServerCertificate = true;

        // Parse the produced connection string and verify individual properties to avoid brittle key-name/order comparisons
        var cs = vm.ConnectionString;
        var builder = new SqlConnectionStringBuilder(cs);

        Assert.AreEqual("localhost", builder.DataSource);
        Assert.AreEqual("db", builder.InitialCatalog);
        Assert.IsFalse(builder.IntegratedSecurity);
        Assert.AreEqual("sa", builder.UserID);
        Assert.AreEqual("p", builder.Password);
        Assert.IsTrue(builder.Encrypt);
        Assert.IsTrue(builder.TrustServerCertificate);
    }

    [TestMethod]
    public void SettingConnectionString_ParsesFieldsCorrectly()
    {
        ConnectionDialogViewModel vm = new ConnectionDialogViewModel();
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
        ConnectionDialogViewModel vm = new ConnectionDialogViewModel();
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
