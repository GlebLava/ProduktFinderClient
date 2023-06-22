using ProduktFinderClient.Components;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProduktFinderClient.Models;

public class LicenseAuthenticator
{
    private static readonly string _baseUrl = @"https://77.24.97.93:7556/";
    //private static readonly string _baseUrl = @"https://localhost:7321/";
    //private static readonly string _baseUrl = @"https://192.168.178.21:7320/"; 
    private static readonly string _authorizeEndpoint = @"pfAuth/";
    private static readonly string _unregisterEndpoint = @"pfUnregisterAuth/";

    private static readonly HttpClient _httpClient = new();

    public static async Task EnsureAuthorization(StatusHandle statusHandle, string licenseKey)
    {

    }

}
