using ArlaNatureConnect.Core.Services;

using System.Net;
using System.Runtime.InteropServices;

namespace TestCore.Services;

[TestClass]
public class GetAddressFromCvrTests
{
    private class FakeHttpClient : GetAddressFromCvr.IHttpClient
    {
        public required string Response { get; set; }
        public string DownloadString(string url) => Response;
    }

    [TestMethod]
    public void GetCompanyInfo_Returns_ApiResult_On_ValidJson()
    {
        string json = "{\"VAT\":\"123\",\"Name\":\"ACME\",\"Address\":\"Street 1\"}";
        FakeHttpClient client = new FakeHttpClient { Response = json };

        GetAddressFromCvr.ApiResult? res = GetAddressFromCvr.GetCompanyInfo("123", client);

        Assert.IsNotNull(res);
        Assert.AreEqual("123", res.VAT);
        Assert.AreEqual("ACME", res.Name);
        Assert.AreEqual("Street 1", res.Address);
    }

    [TestMethod]
    public void GetCompanyInfo_Returns_Null_On_Empty_Response()
    {
        FakeHttpClient client = new FakeHttpClient { Response = string.Empty };

        GetAddressFromCvr.ApiResult? res = GetAddressFromCvr.GetCompanyInfo("123", client);

        Assert.IsNull(res);
    }

    [TestMethod]
    public void GetCompanyInfo_Catches_COMException_Returns_Null()
    {
        FakeHttpClientWithException client = new FakeHttpClientWithException();

        GetAddressFromCvr.ApiResult? res = GetAddressFromCvr.GetCompanyInfo("123", client);

        Assert.IsNull(res);
    }

    private class FakeHttpClientWithException : GetAddressFromCvr.IHttpClient
    {
        public string DownloadString(string url) => throw new COMException();
    }

    [TestMethod]
    public void GetCompanyInfo_Catches_WebException_Returns_Null()
    {
        FakeHttpClientWithWebException client = new FakeHttpClientWithWebException();

        GetAddressFromCvr.ApiResult? res = GetAddressFromCvr.GetCompanyInfo("123", client);

        Assert.IsNull(res);
    }

    private class FakeHttpClientWithWebException : GetAddressFromCvr.IHttpClient
    {
        public string DownloadString(string url) => throw new WebException();
    }

    [TestMethod]
    public void GetCompanyInfo_Deserializes_Owners_And_Productionunits()
    {
        string json = "{\"VAT\":\"123\",\"Name\":\"ACME\",\"Address\":\"Street 1\",\"Owners\":[{\"Name\":\"Owner1\"}],\"Productionunits\":[{\"Address\":\"PU1\"}]}";
        FakeHttpClient client = new FakeHttpClient { Response = json };

        GetAddressFromCvr.ApiResult? res = GetAddressFromCvr.GetCompanyInfo("123", client);

        Assert.IsNotNull(res);
        Assert.IsNotNull(res.Owners);
        Assert.HasCount(1, res.Owners);
        Assert.AreEqual("Owner1", res.Owners[0].Name);
        Assert.IsNotNull(res.Productionunits);
        Assert.HasCount(1, res.Productionunits);
        Assert.AreEqual("PU1", res.Productionunits[0].Address);
    }

    [TestMethod]
    public void GetCompanyInfo_Is_ThreadSafe()
    {
        string json = "{\"VAT\":\"123\",\"Name\":\"ACME\",\"Address\":\"Street 1\"}";
        FakeHttpClient client = new FakeHttpClient { Response = json };

        System.Threading.Tasks.Parallel.For(0, 20, i =>
        {
            GetAddressFromCvr.ApiResult? res = GetAddressFromCvr.GetCompanyInfo(i.ToString(), client);
            Assert.IsNotNull(res);
            Assert.AreEqual("123", res.VAT);
        });
    }

    [TestMethod]
    public void GetCompanyInfo_Online_Search_Cvr_25313763()
    {
        // This test performs a real lookup against the CVR API for CVR 25313763.
        GetAddressFromCvr.ApiResult? res = GetAddressFromCvr.GetCompanyInfo("25313763");

        Assert.IsNotNull(res, "API result should not be null");
        Assert.AreEqual("11823335", res.VAT);
        Assert.IsFalse(string.IsNullOrWhiteSpace(res.Name), "Name should not be empty");
        Assert.IsFalse(string.IsNullOrWhiteSpace(res.Address), "Address should not be empty");
        Assert.IsFalse(string.IsNullOrWhiteSpace(res.Zipcode), "Zipcode should not be empty");
        Assert.IsFalse(string.IsNullOrWhiteSpace(res.City), "City should not be empty");
    }
}
