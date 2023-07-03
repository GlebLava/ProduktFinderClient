

using ProduktFinderClient.Components;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ProduktFinderClient.Models;

public enum AuthenticationResult
{
    AUTHENTICATION_NOT_NEEDED,
    AUTHENTICATION_FAILED_UNAUTHORIZED,
    AUTHENTICATION_FAILED_FORBIDDEN,
    AUTHENTICATION_SUCCEEDED
}

public class AuthenticationHandler
{
    private static readonly string _authorizeEndpoint = @"pfAuth/";
    private static readonly string _unregisterEndpoint = @"pfUnregisterAuth/";

    private static string lastUsedValidLicenseKey = "";
    public static string AuthKey { get; private set; } = "";
    private static SemaphoreSlim authSemaphore = new(1);

    private static readonly HttpClient _authClient;


    static AuthenticationHandler()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        _authClient = new HttpClient(handler);
        AuthKey = LoadSaveSystem.LoadAuthKey();
    }


    public static async Task<AuthenticationResult> HandleAuthenticationIfNecessary(ProduktFinderResponse produktFinderResponse, string licenseKey)
    {
        if (lastUsedValidLicenseKey != licenseKey)
        {
            // We need to try to unregister because if user changes licenseKey from valid to invalid, we get a new one while the authkey is not updated.
            // If we switch back from invalid to the old valid one a new authkey will be requested. Although the old authkey is still regsitered. So the server thinks
            // that two instances are open
            await Unregister();
        }
        else if (produktFinderResponse.ErrorReport is null || produktFinderResponse.ErrorReport.ErrorType != ProduktFinderErrorType.NOT_REGISTERED)
        {
            return AuthenticationResult.AUTHENTICATION_NOT_NEEDED;
        }

        try
        {
            await HandleAuthentication(licenseKey);
        }
        catch (HttpRequestException e2)
        {
            if (e2.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return AuthenticationResult.AUTHENTICATION_FAILED_UNAUTHORIZED;
            }

            if (e2.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return AuthenticationResult.AUTHENTICATION_FAILED_FORBIDDEN;
            }

            throw;
        }

        return AuthenticationResult.AUTHENTICATION_SUCCEEDED;
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="licenseKey"></param>
    /// <returns>new AuthKey</returns>
    private static async Task HandleAuthentication(string licenseKey)
    {
        if (authSemaphore.CurrentCount == 0)
        {
            await authSemaphore.WaitAsync();
            authSemaphore.Release();
            return;
        }

        await authSemaphore.WaitAsync();
        try
        {
            string url = $"{RequestHandler._baseUrl}{_authorizeEndpoint}{licenseKey}";
            var response = await _authClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            AuthKey = JsonSerializer.Deserialize<AuthResponse>(await response.Content.ReadAsStringAsync())!.AuthKey;
            LoadSaveSystem.SaveAuthKey(AuthKey);
            lastUsedValidLicenseKey = licenseKey;
        }
        finally
        {
            authSemaphore.Release();
        }
    }


    public static async Task Unregister()
    {
        string url = $"{RequestHandler._baseUrl}{_unregisterEndpoint}{AuthKey}";
        await _authClient.GetAsync(url);
    }
}
