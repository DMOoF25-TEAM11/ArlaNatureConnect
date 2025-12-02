using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ArlaNatureConnect.Core.Services;

public class GetAddressFromCvr
{
    private const string _API_URL = "https://cvrapi.dk/api?search={0}&country=dk";
    private const string _USER_AGENT = "Studerende UCL - Arla Nature Connect - Jens Tirsvad Nielsen - +601126257062"

    public GetAddressFromCvr()
    {
    }

    public class ApiResult
    {
        public string VAT { get; set; } = null!;
        public required string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Zipcode { get; set; } = null!;
        public string City { get; set; } = null!;
        public bool @protected { get; set; }
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Fax { get; set; } = null!;
        public string Startdate { get; set; } = null!;
        public string Enddate { get; set; } = null!;
        public string Employees { get; set; } = null!;
        public string Addressco { get; set; } = null!;
        public int Industrycode { get; set; }
        public string Industrydesc { get; set; } = null!;
        public int Companycode { get; set; }
        public string Companydesc { get; set; } = null!;
        public string Creditstartdate { get; set; } = null!;
        public int? Creditstatus { get; set; }
        public bool Creditbankrupt { get; set; }
        public ApiOwners[] Owners { get; set; } = null!;
        public ApipPoductionunits[] Productionunits { get; set; } = null!;
        public int T { get; set; }
        public int Version { get; set; }
    }

    public class ApiOwners
    {
        public string Name { get; set; } = null!;
    }

    public class ApipPoductionunits
    {
        public string Address { get; set; } = null!;
    }

    public interface IHttpClient
    {
        string DownloadString(string url);
    }

    private class DefaultHttpClient : IHttpClient
    {
        public string DownloadString(string url)
        {
            // Use HttpClient to ensure TLS and explicit status handling
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_USER_AGENT);
                httpClient.Timeout = TimeSpan.FromSeconds(15);
                HttpResponseMessage response = httpClient.GetAsync(url).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Get company info from CVR API. Accepts optional IHttpClient for testability.
    /// Returns null when a COM error is encountered or null/empty response.
    /// </summary>
    public static ApiResult? GetCompanyInfo(string name, IHttpClient? httpClient = null)
    {
        httpClient ??= new DefaultHttpClient();
        try
        {
            string resultContent = httpClient.DownloadString(string.Format(_API_URL, name));
            if (string.IsNullOrWhiteSpace(resultContent))
                return null;
            JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<ApiResult>(resultContent, options);
        }
        catch (COMException)
        {
            // Handle COM interop errors from serializer or underlying calls gracefully for tests
            return null;
        }
        catch (WebException)
        {
            // network errors - bubble up or return null depending on callers; return null for tests
            return null;
        }
        catch (HttpRequestException)
        {
            // network errors from HttpClient - return null for tests
            return null;
        }
    }
}
