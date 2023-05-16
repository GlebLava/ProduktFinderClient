using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using ProduktFinderClient.Components;
using ProduktFinderClient.DataTypes;
using ProduktFinderClient.Models.ErrorLogging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
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
        DIGI_KEY = 6,
    }

    public class ModuleTranslations
    {
        public HashSet<ModuleType> ModulesToSearchWith { get; set; } = new HashSet<ModuleType>();

        [NonSerialized]
        public static readonly BidirectionalDictionary<ModuleType, string> ModulesTranslation;

        // Just so we can init ModulesTranslation
        static ModuleTranslations()
        {
            ModulesTranslation = new BidirectionalDictionary<ModuleType, string>();
            ModulesTranslation.Add(ModuleType.MOUSER, "Mouser");
            ModulesTranslation.Add(ModuleType.FARNELL, "Farnell");
            ModulesTranslation.Add(ModuleType.FUTURE, "Future");
            ModulesTranslation.Add(ModuleType.MYARROW, "MyArrow");
            ModulesTranslation.Add(ModuleType.SCHUKAT, "Schukat");
            ModulesTranslation.Add(ModuleType.REICHELT, "Reichelt");
            ModulesTranslation.Add(ModuleType.DIGI_KEY, "DigiKey");
        }

        public static List<string> GetModuleNamesList()
        {
            List<string> list = new();
            foreach (ModuleType moduleType in Enum.GetValues(typeof(ModuleType)))
            {
                ModulesTranslation.TryGetValue(moduleType, out string moduleString);
                list.Add(moduleString);
            }

            return list;
        }

    }

    public class RequestHandler
    {
        /// <summary>
        /// VERY IMPORTANT: Https can only be used with valid certs in production
        /// </summary>

        //private static readonly string _connectionUrl = "http://localhost:7322/ProduktFinderHub/";
        private static readonly string _connectionUrl = "http://192.168.178.24:7322/ProduktFinderHub/";

        private static HubConnection? connection;
        private static readonly SemaphoreSlim _semaphore;
        private static readonly SemaphoreSlim _connectionSemaphore = new(1); //Only here for locking the building of the connection
        private static string lastUsedLicenseKey = "";
        private static Exception? lastException;

        static RequestHandler()
        {
            _semaphore = new SemaphoreSlim(10);
        }

        public static async Task SearchWith(string licenseKey, string keyword, ModuleType api, int numberOfResultsPerAPI, StatusHandle statusHandle, Action<List<Part>?> OnSearchFinishedCallback, CancellationToken cancellationToken)
        {
            var result = await SearchWith(licenseKey, keyword, api, numberOfResultsPerAPI, statusHandle, cancellationToken);


            if (!cancellationToken.IsCancellationRequested)
                OnSearchFinishedCallback(result);
        }


        public static async Task<List<Part>?> SearchWith(string licenseKey, string keyword, ModuleType api, int numberOfResultsPerAPI, StatusHandle statusHandle, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

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

                await EnsureConnection(licenseKey);
                // If server is down this throws InvalidOperationException
                ProduktFinderResponse? produktFinderResponse =
                    await connection!.InvokeAsync<ProduktFinderResponse?>("SearchWith", input, cancellationToken);

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
            catch (WebSocketException)
            {
                UpdateUserError(statusHandle, "Verbindung zum Server fehlgeschlagen. Versuchen Sie es später neu", keyword);
                return null;
            }
            catch (InvalidOperationException)
            {
                if (lastException is HubException he)
                    HandleHubException(he, statusHandle, keyword);
                else
                    UpdateUserError(statusHandle, "Verbindung zum Server fehlgeschlagen. Versuchen Sie es später neu", keyword);

                return null;
            }
            catch (HubException e)
            {
                HandleHubException(e, statusHandle, keyword);
                return null;
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e, keyword);
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Throws websocketexception if the handshake wasnt met
        /// </summary>
        /// <param name="licenseKey"></param>
        /// <returns></returns>
        private static async Task EnsureConnection(string licenseKey)
        {
            if (connection is not null && connection.State == HubConnectionState.Disconnected)
                await Task.Delay(1000);


            _connectionSemaphore.Wait();
            try
            {
                if (connection is null || connection.State == HubConnectionState.Disconnected || lastUsedLicenseKey != licenseKey)
                {
                    connection = new HubConnectionBuilder()
                            .WithUrl(_connectionUrl, options =>
                            {
                                options.Headers["VmRWfFt23B"] = Encode(licenseKey);
                            })
                            .Build();


                    /*
                      To see what is happening here see https://github.com/dotnet/aspnetcore/issues/10766
                      Although await _connection.StartAsync() should block until the connection is received, it does not.
                      We try to do the same thing here as the ThreadOwner (see link)
                      Although we solve it in a different way, because the solution proposed throws the wrong Exception. We use
                      the Exception Message to differentiate between too many Instances and wrong licence key

                      To solve this lastException is used.
                      Everywhere where an SignalR related Exception is thrown, we check for lastException first. If lastException is
                      a HubException, then StartAsync failed related to licenseKey.
                    */
                    connection.Closed += OnConnectionClosed;
                    //If server is down this throws httpRequestException
                    lastException = null;
                    await connection.StartAsync();
                    lastUsedLicenseKey = licenseKey;
                }
            }
            // This needs to be here, else any Exceptions dont get thrown
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        private static string Encode(string stringToEncode)
        {
            return stringToEncode;
        }


        /// <summary>
        /// Reason for this method is that we can subscribe it to _connection.Closed
        /// If we didnt a random error would be thrown if the server shuts down
        /// and disconnects unexpectedly, when our socket polls if the connection is still
        /// alive.
        /// This method does not do anything since SearchWith handles the automatic connection
        /// anyway witht the help of EnsureConnection
        /// </summary>
        private static async Task OnConnectionClosed(Exception? e)
        {
            lastException = e;
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

        private static void HandleHubException(HubException e, StatusHandle statusHandle, string keyword)
        {
            if (e.Message.Contains("Not Authorized"))
                UpdateUserError(statusHandle, "Falscher Lizensschlüssel. Aktualisieren sie Ihren Lizensschlüssel in den Optionen", keyword);

            if (e.Message.Contains("Too many instances"))
                UpdateUserError(statusHandle, "Sie haben zu viele Produktfinder offen. Schließen Sie eine der Instanzen um diese benutzen zu können", keyword);
        }

    }

    public class ProduktFinderParams
    {
        public int MaxPart { get; set; }
        public ModuleType ModuleType { get; set; }
        public string KeyWord { get; set; } = string.Empty;
    }

}
