using ProduktFinderClient.Components;
using ProduktFinderClient.Models.ErrorLogging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ProduktFinderClient.Models;

public class RequestHandler
{
    private static readonly HttpClientQueue _httpQueue = new(10);
    private static readonly HttpClient _authClient;
    //private static readonly string _baseUrl = @"https://77.24.97.93:7556/";
    private static readonly string _baseUrl = @"https://localhost:7321/";
    //private static readonly string _baseUrl = @"https://192.168.178.21:7320/";
    private static readonly string _getPartsEndpoint = @"getParts/";
    private static readonly string _authorizeEndpoint = @"pfAuth/";
    private static readonly string _unregisterEndpoint = @"pfUnregisterAuth/";

    private static string authKey = "";
    private static SemaphoreSlim authSemaphore = new(1);

    static RequestHandler()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        _authClient = new HttpClient(handler);
        authKey = LoadSaveSystem.LoadAuthKey();
    }

    public static async Task SearchWith(string licenseKey, string keyword, ModuleType api, int numberOfResultsPerAPI, StatusHandle statusHandle, Action<List<Part>?> OnSearchFinishedCallback, CancellationToken cancellationToken)
    {
        var result = await SearchWith(licenseKey, keyword, api, numberOfResultsPerAPI, statusHandle, cancellationToken);

        if (!cancellationToken.IsCancellationRequested)
            OnSearchFinishedCallback(result);
    }

    /// <summary>
    /// God forgive me for I have sinned. 
    /// For anyone else in the future, looking at this. Brace your eyes
    /// </summary>
    /// <param name="licenseKey"></param>
    /// <param name="keyword"></param>
    /// <param name="api"></param>
    /// <param name="numberOfResultsPerAPI"></param>
    /// <param name="statusHandle"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<Part>?> SearchWith(string licenseKey, string keyword, ModuleType api, int numberOfResultsPerAPI, StatusHandle statusHandle, CancellationToken cancellationToken)
    {
        try
        {
            keyword = FilterKeyWord(keyword);
            InitializeNewStatusHandle(api, statusHandle, keyword);

            HttpResponseMessage response;
            try
            {
                SearchWithPostParams input = new() { AuthKey = authKey, KeyWord = keyword, MaxPart = numberOfResultsPerAPI, ModuleType = api };
                response = await GetPostResponse(_baseUrl + _getPartsEndpoint, input, cancellationToken);
            }
            catch (HttpRequestException)
            {
                try
                {
                    await HandleAuthentication(licenseKey);
                }
                catch (HttpRequestException e)
                {
                    if (e.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        UpdateUserError(statusHandle, "Linzensschlüssel ist nicht gültig. Man kann den Lizensschlüssen in den Optionen finden", keyword);
                        MessageBox.Show("Linzensschlüssel ist nicht gültig. Man kann den Lizensschlüssen in den Optionen finden");
                        return null;
                    }

                    if (e.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        UpdateUserError(statusHandle, "Ein unter diesem Lizenschlüssel registrierter Produktfinder ist schon offen." +
                                                      " Um diesen benutzen zu können müssen Sie den anderen erstmal schließen", keyword);
                        MessageBox.Show("Ein unter diesem Lizenschlüssel registrierter Produktfinder ist schon offen." +
                                                      " Um diesen benutzen zu können müssen Sie den anderen erstmal schließen");
                        return null;
                    }

                    throw;
                }
                // authKey CAN BE CHANGED BY HANDKEAUTHENTICATION
                SearchWithPostParams input = new() { AuthKey = authKey, KeyWord = keyword, MaxPart = numberOfResultsPerAPI, ModuleType = api };
                response = await GetPostResponse(_baseUrl + _getPartsEndpoint, input, cancellationToken); // Only try once after trying to authenticate
            }

            string answer = await response.Content.ReadAsStringAsync();
            ProduktFinderResponse? produktFinderResponse = JsonSerializer.Deserialize<ProduktFinderResponse>(answer, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (produktFinderResponse is null)
            {
                UpdateUserError(statusHandle, "Bei dem Modul trat ein Problem auf. Keine Antwort bekommen", keyword);
                return null;
            }

            if (produktFinderResponse.ErrorReport is not null)
            {
                UpdateUserError(statusHandle, produktFinderResponse.ErrorReport.ErrorDescription, keyword);
                return null;
            }

            List<Part> results = produktFinderResponse.Parts;
            statusHandle.TextRight = $" Suche fertig {results.Count} Teile gefunden";
            statusHandle.ColorRight = Colors.Green;
            return results;
        }
        catch (HttpRequestException)
        {
            UpdateUserError(statusHandle, "Verbindung zum Server fehlgeschlagen. Versuchen Sie es später neu", keyword);
            return null;
        }
        catch (TaskCanceledException)
        {
            UpdateUserError(statusHandle, "Suche abgebrochen", keyword);
            return null;
        }
        catch (OperationCanceledException)
        {
            UpdateUserError(statusHandle, "Suche abgebrochen", keyword);
            return null;
        }
        catch (Exception e)
        {
            ErrorLogger.LogError(e, keyword);
            return null;
        }
    }

    /// <summary>
    /// also throws if there is no SuccessStatusCode on the response
    /// </summary>
    /// <param name="url"></param>
    /// <param name="input"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException">throws this if there is some unexpected Problem with the httpQueue</exception>
    private static async Task<HttpResponseMessage> GetPostResponse(string url, object input, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");
        HttpResponseMessage? response = await _httpQueue.EnqueueAsync(request, cancellationToken);

        if (response is null)
            throw new HttpRequestException("Problem with the HttpQueue");

        response.EnsureSuccessStatusCode();
        return response;
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
            string url = $"{_baseUrl}{_authorizeEndpoint}{licenseKey}";
            var response = await _authClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            authKey = JsonSerializer.Deserialize<AuthResponse>(await response.Content.ReadAsStringAsync())!.AuthKey;
            LoadSaveSystem.SaveAuthKey(authKey);
        }
        finally
        {
            authSemaphore.Release();
        }
    }

    private static void InitializeNewStatusHandle(ModuleType moduleType, StatusHandle statusHandle, string keyword)
    {
        ModuleTranslations.ModulesTranslation.TryGetValue(moduleType, out string moduleName);
        DateTime currentTime = DateTime.Now;
        string formattedTime = currentTime.ToString("HH:mm");
        statusHandle.TextLeft = $"<{formattedTime}> {moduleName} \"{keyword}\" ";
    }


    private static void UpdateUserError(StatusHandle statusHandle, string message, string keyword)
    {
        statusHandle.ColorRight = Colors.Red;
        statusHandle.TextRight = " " + message;
    }

    public static async Task Unregister()
    {
        string url = $"{_baseUrl}{_unregisterEndpoint}{authKey}";
        await _authClient.GetAsync(url);
    }

    private static string FilterKeyWord(string keyWord)
    {
        if (keyWord is null) return "";

        string result = keyWord.Replace("\\", "");
        result = result.Replace("/", "");

        return result;
    }
}

public class AuthResponse
{
    public string AuthKey { get; set; } = string.Empty;
}

public class SearchWithPostParams
{
    public string AuthKey { get; set; } = string.Empty;
    public int MaxPart { get; set; }
    public ModuleType ModuleType { get; set; }
    public string KeyWord { get; set; } = string.Empty;
}

