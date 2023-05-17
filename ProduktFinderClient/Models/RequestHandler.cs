using ProduktFinderClient.Components;
using ProduktFinderClient.Models.ErrorLogging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ProduktFinderClient.Models;

public class RequestHandler
{
    private static readonly HttpClientQueue _httpQueue = new(10);
    private static readonly string _baseUrl = @"https://77.24.97.93:7556/getParts/";
    //private static readonly string _baseUrl = @"https://localhost:7321/getParts/";

    public static async Task SearchWith(string licenseKey, string keyword, ModuleType api, int numberOfResultsPerAPI, StatusHandle statusHandle, Action<List<Part>?> OnSearchFinishedCallback, CancellationToken cancellationToken)
    {
        var result = await SearchWith(licenseKey, keyword, api, numberOfResultsPerAPI, statusHandle, cancellationToken);


        if (!cancellationToken.IsCancellationRequested)
            OnSearchFinishedCallback(result);
    }


    public static async Task<List<Part>?> SearchWith(string licenseKey, string keyword, ModuleType api, int numberOfResultsPerAPI, StatusHandle statusHandle, CancellationToken cancellationToken)
    {
        try
        {
            keyword = FilterKeyWord(keyword);

            // UPDATE USER
            ModuleTranslations.ModulesTranslation.TryGetValue(api, out string moduleName);
            DateTime currentTime = DateTime.Now;
            string formattedTime = currentTime.ToString("HH:mm");
            statusHandle.TextLeft = $"<{formattedTime}> {moduleName} \"{keyword}\" ";
            // UPDATE USER END

            ProduktFinderParams input = new() { KeyWord = keyword, MaxPart = numberOfResultsPerAPI, ModuleType = api };

            string url = _baseUrl + keyword + ";" + numberOfResultsPerAPI.ToString() + ";" + api;

            HttpRequestMessage request = new(HttpMethod.Get, url);
            HttpResponseMessage? response = await _httpQueue.EnqueueAsync(request, cancellationToken);

            if (response is null)
                throw new Exception("Problem with the HttpQueue");

            if (!CheckErrorCodes(response))
            {
                UpdateUserError(statusHandle, "Bei dem Modul trat ein Problem auf. Keine Antwort bekommen", keyword);
                return null;
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
        catch (Exception e)
        {
            ErrorLogger.LogError(e, keyword);
            return null;
        }
    }

    private static string Encode(string stringToEncode)
    {
        return stringToEncode;
    }
    private static bool CheckErrorCodes(HttpResponseMessage responseMessage)
    {
        if (responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            return false;
        if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;

        return true;
    }

    private static void UpdateUserError(StatusHandle statusHandle, string message, string keyword)
    {
        statusHandle.ColorRight = Colors.Red;
        statusHandle.TextRight = " " + message;
    }

    private static string FilterKeyWord(string keyWord)
    {
        if (keyWord is null) return "";

        string result = keyWord.Replace("\\", "");
        result = result.Replace("/", "");

        return result;
    }
}

public class ProduktFinderParams
{
    public int MaxPart { get; set; }
    public ModuleType ModuleType { get; set; }
    public string KeyWord { get; set; } = string.Empty;
}

