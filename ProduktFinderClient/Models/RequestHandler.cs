using ProduktFinderClient.Models.ErrorLogging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text.Json;
using ProduktFinderClient.DataTypes;


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
        private static readonly string _baseUrl = @"http://localhost:7322/getParts/";



        public static async Task SearchWith(string keyword, ModuleType api, int numberOfResultsPerAPI, Action<string> UpdateUserCallback, Action<List<Part>?> OnSearchFinishedCallback)
        {
            var result = await SearchWith(keyword, api, numberOfResultsPerAPI, UpdateUserCallback);
            OnSearchFinishedCallback(result);
        }


        public static async Task<List<Part>?> SearchWith(string keyword, ModuleType api, int numberOfResultsPerAPI, Action<string> UpdateUserCallback)
        {
            try
            {
                keyword = FilterKeyWord(keyword);

                // UPDATE USER
                Filter.ModulesTranslation.TryGetValue(api, out string moduleName);

                UpdateUserCallback?.Invoke("Am Suchen mit " + moduleName);
                // UPDATE USER END

                string url = _baseUrl + keyword + ";" + numberOfResultsPerAPI.ToString() + ";" + api;

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                HttpResponseMessage? response = await _httpQueue.EnqueueAsync(request);
                if (response is null)
                    throw new Exception("Problem with the HttpQueue");

                if (CheckErrorCodes(response))
                {
                    string answer = await response.Content.ReadAsStringAsync();
                    List<Part>? results = JsonSerializer.Deserialize<List<Part>>(answer, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });


                    UpdateUserCallback?.Invoke(moduleName + "  Suche fertig");
                    return results;
                }
                else
                {
                    UpdateUserCallback?.Invoke(moduleName + "  hatte Probleme. Keine Antwort bekommen");
                    return new List<Part>();
                }
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e, keyword);
                return null;
            }

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
