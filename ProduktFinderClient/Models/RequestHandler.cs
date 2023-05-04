using ProduktFinderClient.Components;
using ProduktFinderClient.DataTypes;
using ProduktFinderClient.Models.ErrorLogging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ProduktFinderClient.Models
{
    public enum ModuleType
    {
        MOUSER = 0,
        FARNELL = 1,
        FUTURE = 2,
        MYARROW = 3,
        SCHUKAT = 4,
        REICHELT = 5,
    }

    public class Filter
    {
        public HashSet<ModuleType> ModulesToSearchWith { get; set; } = new HashSet<ModuleType>();

        [NonSerialized]
        public static readonly BidirectionalDictionary<ModuleType, string> ModulesTranslation;



        // Just so we can init ModulesTranslation
        static Filter()
        {
            ModulesTranslation = new BidirectionalDictionary<ModuleType, string>();
            ModulesTranslation.Add(ModuleType.MOUSER, "Mouser");
            ModulesTranslation.Add(ModuleType.FARNELL, "Farnell");
            ModulesTranslation.Add(ModuleType.FUTURE, "Future");
            ModulesTranslation.Add(ModuleType.MYARROW, "MyArrow");
            ModulesTranslation.Add(ModuleType.SCHUKAT, "Schukat");
            ModulesTranslation.Add(ModuleType.REICHELT, "Reichelt");
        }
    }

    public class RequestHandler
    {
        private static readonly HttpClientQueue _httpQueue = new(10);
        //private static readonly string _baseUrl = @"http://77.24.97.93:7555/getParts/";
        private static readonly string _baseUrl = @"https://localhost:7321/getParts/";



        public static async Task SearchWith(string keyword, ModuleType api, int numberOfResultsPerAPI, StatusHandle statusHandle, Action<List<Part>?> OnSearchFinishedCallback, CancellationToken cancellationToken)
        {
            var result = await SearchWith(keyword, api, numberOfResultsPerAPI, statusHandle, cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
                OnSearchFinishedCallback(result);
        }


        public static async Task<List<Part>?> SearchWith(string keyword, ModuleType api, int numberOfResultsPerAPI, StatusHandle statusHandle, CancellationToken cancellationToken)
        {
            try
            {
                keyword = FilterKeyWord(keyword);

                // UPDATE USER
                Filter.ModulesTranslation.TryGetValue(api, out string moduleName);
                DateTime currentTime = DateTime.Now;
                string formattedTime = currentTime.ToString("HH:mm");
                statusHandle.TextLeft = $"<{formattedTime}> {moduleName} \"{keyword}\" ";
                // UPDATE USER END


                string url = _baseUrl + keyword + ";" + numberOfResultsPerAPI.ToString() + ";" + api;

                HttpRequestMessage request = new(HttpMethod.Get, url);
                HttpResponseMessage? response = null;

                try
                {
                    response = await _httpQueue.EnqueueAsync(request, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    UpdateUserError(statusHandle, "Suche Abbgebrochen", keyword);
                    return null;
                }


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
            catch (Exception e)
            {
                ErrorLogger.LogError(e, keyword);
                return null;
            }
        }

        private static void UpdateUserError(StatusHandle statusHandle, string message, string keyword)
        {
            statusHandle.ColorRight = Colors.Red;
            statusHandle.TextRight = " " + message;
        }


        private static bool CheckErrorCodes(HttpResponseMessage responseMessage)
        {
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                return false;
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                return false;

            return true;
        }

        private static string FilterKeyWord(string keyWord)
        {
            if (keyWord is null) return "";

            string result = keyWord.Replace("\\", "");
            result = result.Replace("/", "");

            return result;
        }
    }


}
