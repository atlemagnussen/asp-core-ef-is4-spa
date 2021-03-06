﻿using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Test.consoleapp
{
    public static class TestAuthClients
    {
        //private static string AuthServerUrl = "https://localhost:6001";
        //private static readonly string ApiBaseUrl = "https://localhost:7001/api/";

        private static readonly string AuthServerUrl = "https://asp-core-auth-server.azurewebsites.net";
        private static readonly string ApiBaseUrl = "https://asp-core-webapi.azurewebsites.net/api/";
        public static async Task Do()
        {
            // discover all the endpoints using metadata of identity server
            var client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl)
            };
            var disco = await client.GetDiscoveryDocumentAsync(AuthServerUrl);
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            var tokenResponse = await GetTokenClientCred(client, disco);
            //var tokenResponse = await GetTokenResourceOwner(client, disco, "atlemagnussen@gmail.com", "Einherjer57!");

            client.SetBearerToken(tokenResponse.AccessToken);

            //await CreateCustomer(client, "From", "Console");
            await ListAll(client);
        }

        private static async Task ListAll(HttpClient client)
        {
            var customersResponse = await client.GetAsync("customers");

            if (!customersResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"statusCode={customersResponse.StatusCode}");
                Console.WriteLine($"reason={customersResponse.ReasonPhrase}");
            }
            else
            {
                Console.WriteLine($"Request OK, statusCode={customersResponse.StatusCode}");
                var content = await customersResponse.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }

        private static async Task CreateCustomer(HttpClient client, string first, string last)
        {
            var customerInfo = new StringContent(
                JsonConvert.SerializeObject(
                    new { FirstName = first, LastName = last }), Encoding.UTF8, "application/json");

            var createCustomerResponse = await client.PostAsync("customers", customerInfo);

            if (!createCustomerResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"statusCode={createCustomerResponse.StatusCode}");
                Console.WriteLine($"reason={createCustomerResponse.ReasonPhrase}");
            }
            else
            {
                Console.WriteLine(createCustomerResponse.StatusCode);
                var content = await createCustomerResponse.Content.ReadAsStringAsync();
                Console.WriteLine(JObject.Parse(content));
            }
        }

        private static async Task<TokenResponse> GetTokenResourceOwner(HttpClient client, DiscoveryDocumentResponse disco, string user, string pass)
        {
            // Grab a bearer token
            var tokenOptions = new TokenClientOptions
            {
                Address = disco.TokenEndpoint,
                ClientId = "ro.client",
                ClientSecret = "secret"
            };
            var tokenClient = new TokenClient(client, tokenOptions);
            var tokenResponse = await tokenClient.RequestPasswordTokenAsync(user, pass, "bankApi");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
            }

            Console.WriteLine("Got token:");
            Console.WriteLine(tokenResponse.Json);
            return tokenResponse;
        }

        private static async Task<TokenResponse> GetTokenClientCred(HttpClient client, DiscoveryDocumentResponse disco)
        {
            // Grab a bearer token
            var tokenOptions = new TokenClientOptions
            {
                ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader,
                Address = disco.TokenEndpoint,
                ClientId = "MyBeastClient",
                ClientSecret = "xaUgjy9Qsps+WVRV0JjnX0mmIANPAInlqZT8v1QRDa8=" // "y59VugLnd02AP1vNXj6P +9gSpL9vhnY0uUVQ7uSWD5s="
            };
            var tokenClient = new TokenClient(client, tokenOptions);
            var tokenResponse = await tokenClient.RequestClientCredentialsTokenAsync("bankApi");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
            }

            Console.WriteLine("Got token:");
            Console.WriteLine(tokenResponse.Json);
            return tokenResponse;
        }
    }
}
