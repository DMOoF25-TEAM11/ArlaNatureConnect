namespace ArlaNatureConnect.Core.Services;

/// <summary>
/// Contract for looking up company information from the CVR API.
/// </summary>
/// <remarks>
/// Inheriting XML documentation with <c>&lt;inheritdoc/&gt;</c>:
/// - Document the behavior on the contract (interface or base class).
/// - Implement the contract in an implementing type and place <c>&lt;inheritdoc/&gt;</c> on the implementing member.
/// - The implementation will automatically inherit the documentation from the contract, keeping docs DRY
///   and ensuring the contract remains the single source of truth for behavior and remarks.
///
/// Why we have it:
/// - Avoids duplicating the same XML comments on every implementation.
/// - Keeps documentation consistent between interface and implementations.
///
/// Code example:
/// <example>
/// <code language="csharp">
/// // The interface documents the contract:
/// public interface IGetAddressFromCvr
/// {
///     /// <summary>
///     /// Get company info from CVR API.
///     /// </summary>
///     static abstract GetAddressFromCvr.ApiResult? GetCompanyInfo(string name, GetAddressFromCvr.IHttpClient? httpClient = null);
/// }
///
/// // An implementing type can inherit the documentation using <inheritdoc/>:
/// public class MyCvrLookup : IGetAddressFromCvr
/// {
///     /// <inheritdoc/>
///     public static GetAddressFromCvr.ApiResult? GetCompanyInfo(string name, GetAddressFromCvr.IHttpClient? httpClient = null)
///     {
///         // Forward to shared implementation
///         return GetAddressFromCvr.GetCompanyInfo(name, httpClient);
///     }
/// }
///
/// // Usage
/// var info = MyCvrLookup.GetCompanyInfo("Arla Foods");
/// </code>
/// </example>
/// </remarks>
public interface IGetAddressFromCvr
{
    // Inline note: the static abstract member lets types provide a static implementation that can be
    // called via generic constraints or directly on the implementing type.
    static abstract GetAddressFromCvr.ApiResult? GetCompanyInfo(string name, GetAddressFromCvr.IHttpClient? httpClient = null);
}