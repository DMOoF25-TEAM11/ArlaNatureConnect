using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ArlaNatureConnect.Core.Services;

/// <summary>
/// Service for looking up company information from the CVR API.
/// </summary>
/// <remarks>
/// This class contains helpers to call the external CVR API and deserialize the response into a strongly-typed
/// <see cref="ApiResult"/> instance.
///
/// Inheriting XML documentation with <c>&lt;inheritdoc/&gt;</c>:
/// - Define the contract and document it (for example an interface method).
/// - Implement the contract in a class and place <c>&lt;inheritdoc/&gt;</c> on the implementing member.
/// - The documentation will be inherited from the contract (interface or base class) which keeps docs DRY
///   and consistent between the contract and implementations.
///
/// Why we have it:
/// - Reduces duplication when multiple implementations must share the same API documentation.
/// - Keeps the contract (interface/base class) as the single source of truth for behavior and remarks.
///
/// Code example:
/// <example>
/// <code language="csharp">
/// // Interface defines the behavior and documents it:
/// public interface IHttpClient
/// {
///     /// <summary>
///     /// Download the content from <paramref name="url"/> and return it as a string.
///     /// </summary>
///     /// <param name="url">The url to fetch.</param>
///     /// <returns>Content as string.</returns>
///     string DownloadString(string url);
/// }
///
/// // Implementation can inherit the XML docs from the interface:
/// public class DefaultHttpClient : IHttpClient
/// {
///     /// <inheritdoc/>
///     public string DownloadString(string url)
///     {
///         // actual implementation
///     }
/// }
///
/// // Usage example
/// var info = GetAddressFromCvr.GetCompanyInfo("Arla Foods");
/// </code>
/// </example>
/// </remarks>
public class GetAddressFromCvr() : IGetAddressFromCvr
{
    private const string _API_URL = "https://cvrapi.dk/api?search={0}&country=dk";
    private const string _USER_AGENT = "Student UCL - Arla Nature Connect - Jens Tirsvad Nielsen - +601126257062";

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

    /// <summary>
    /// Abstraction for downloading a URL to a string. Document the contract here so implementations can inherit
    /// documentation with <c>&lt;inheritdoc/&gt;</c>.
    /// </summary>
    /// <param name="url">The URL to download.</param>
    /// <returns>The response body as a string.</returns>
    public interface IHttpClient
    {
        string DownloadString(string url);
    }

    private class DefaultHttpClient : IHttpClient
    {
        /// <inheritdoc/>
        public string DownloadString(string url)
        {
            // Use HttpClient to ensure TLS and explicit status handling
            // Inline note: normally reusing a single HttpClient instance is recommended to avoid socket exhaustion,
            // but here we create a short-lived instance for simplicity and to keep behavior deterministic in tests.
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
    /// Get company info from CVR API. Accepts optional <see cref="IHttpClient"/> for testability.
    /// Returns null when a COM error is encountered or null/empty response.
    /// </summary>
    /// <param name="name">Company name or VAT to search for.</param>
    /// <param name="httpClient">Optional HTTP client implementation (useful for testing).</param>
    /// <returns>An <see cref="ApiResult"/> instance when the API returns data; otherwise null.</returns>
    /// <example>
    /// <code language="csharp">
    /// // Simple call using the default client
    /// var result = GetAddressFromCvr.GetCompanyInfo("Arla Foods");
    /// if (result != null)
    /// {
    ///     Console.WriteLine(result.Name);
    /// }
    ///
    /// // In tests you can inject a fake IHttpClient that returns prepared JSON
    /// var fake = new MyFakeHttpClient("{ \"name\": \"MyCompany\" }");
    /// var result2 = GetAddressFromCvr.GetCompanyInfo("MyCompany", fake);
    /// </code>
    /// </example>
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
