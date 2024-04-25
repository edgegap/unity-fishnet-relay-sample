using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FishNet.Transporting.KCP.Edgegap;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public static class EdgegapRelayService
{
    private static readonly HttpClient _httpClient = new HttpClient();
    private static string _relayProfileToken;
    private const string API_URL = "https://api.edgegap.com";

    public static void Initialize(string relayToken)
    {
        _relayProfileToken = relayToken;
    }
    
    public static async Task<ApiResponse> CreateSessionAsync(int numPlayer)
    {
        // Set the authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", _relayProfileToken);
        //Set the Ips for the request
        var objectToSerialize = new RootObject
        {
            users = new List<Users>()
        };

        //For the sake of a demo, I hardcoded Ips **EDIT using IP lookup instead, otherwise the session is not authorized
        for (var i = 0; i < numPlayer; i++)
        {
            HttpResponseMessage ipRes = await _httpClient.GetAsync($"{API_URL}/v1/ip");
            string ipContent = await ipRes.Content.ReadAsStringAsync();
            var ipData = JsonConvert.DeserializeObject<PublicIP>(ipContent);

            var user = new Users { ip = ipData.public_ip };
            objectToSerialize.users.Add(user);
        }
        
        // Serialize the IP array to JSON
        var jsonContent = new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");
        // Send the POST request and get the response
        HttpResponseMessage response = await _httpClient.PostAsync($"{API_URL}/v1/relays/sessions", jsonContent);
        
        string responseContent = await response.Content.ReadAsStringAsync();
        Debug.Log(responseContent);
        //Deserialize the response of the API
        var content = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
        
        //Sends a loop to wait for a positive responce
        //The first answer of the API contain very few informations, but with the session_id that it gives us,
        //we can find our session and wait for it to be ready
        await PollDataAsync(_httpClient, content, content.session_id);
        //Reinitialize our content
        HttpResponseMessage newResponse = await _httpClient.GetAsync($"{API_URL}/v1/relays/sessions/" + content.session_id);
        string newResponseContent = await newResponse.Content.ReadAsStringAsync();
        var data = JsonConvert.DeserializeObject<ApiResponse>(newResponseContent);
        
        if (data.ready)
        {
            return data;
        }
        
        throw new InvalidOperationException($"Error: {response.RequestMessage} - {response.ReasonPhrase}"
                                            + "\nError: Couldn't found a session relay");
    }

    private static async Task PollDataAsync(HttpClient client, ApiResponse content, string sessionId)
    {
        //TODO say something when waiting for too long
        while (!content.ready)
        {
            Debug.Log("Waiting for data to be ready...");
            await Task.Delay(3000); // Wait 3 seconds between each iteration
            var response = await client.GetAsync($"{API_URL}/v1/relays/sessions/" + sessionId);
            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.Log("Response from client -----------" + responseContent);
            content = JsonConvert.DeserializeObject<ApiResponse>(responseContent);
            Debug.Log("Is the game ready : " + content.ready);
        }

        // The "ready" property is now true, output a message
        Debug.Log("Data is now ready!");
    }

    public static async Task<ApiResponse> JoinSessionAsync(string sessionId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", _relayProfileToken);
        HttpResponseMessage response = await _httpClient.GetAsync($"{API_URL}/v1/relays/sessions/" + sessionId);
        //Catch bad session ID
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Error: {response.RequestMessage} - {response.ReasonPhrase}");
        }
        string responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ApiResponse>(responseContent);
    }
}