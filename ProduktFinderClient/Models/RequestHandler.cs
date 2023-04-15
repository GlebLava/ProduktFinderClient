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
        }
    }

    public class RequestHandler
    {
        private static readonly HttpClient _httpClient = new();
        private static readonly string _baseUrl = @"https://localhost:7295/getParts/";



        public static async Task SearchWith(ModuleType api, string keyword, int numberOfResultsPerAPI, Action<string> UpdateUserCallback, Action<List<Part>?> OnSearchFinishedCallback)
        {
            var result = await SearchWith(api, keyword, numberOfResultsPerAPI, UpdateUserCallback);
            OnSearchFinishedCallback(result);
        }


        public static async Task<List<Part>?> SearchWith(ModuleType api, string keyword, int numberOfResultsPerAPI, Action<string> UpdateUserCallback)
        {
            Filter filter = new Filter();
            filter.ModulesToSearchWith.Add(api);

            return await Search(keyword ,filter, numberOfResultsPerAPI, UpdateUserCallback);
        }

        public static async Task<List<Part>?> Search(string keyword, Filter filter, int numberOfResultsPerAPI, Action<string> UpdateUserCallback)
        {
            try
            {
                string userCallbackModules = "";
                foreach (var module in filter.ModulesToSearchWith)
                    userCallbackModules += module.ToString() + ", ";
                UpdateUserCallback?.Invoke("Am Suchen mit " + userCallbackModules);

                string sContent = JsonSerializer.Serialize<Filter>(filter);
                StringContent stringContent = new StringContent(sContent, System.Text.Encoding.UTF8, "application/json");
                string url = _baseUrl + keyword + ";" + numberOfResultsPerAPI.ToString();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = stringContent;

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                string answer = await response.Content.ReadAsStringAsync();
                List<Part>? results = JsonSerializer.Deserialize<List<Part>>(answer, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                UpdateUserCallback?.Invoke(userCallbackModules + "  Suche fertig");
                return results;
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e, keyword);
                return null;
            }
            
        }
    }


}
