using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Collections;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace jsonrpc;

public partial class RPCClient
{
    private string? serviceEndpoint
    {
        get;set;
    }
    private readonly Dictionary<HttpRequestMessage, List<RPCRequest>> requests;
    private readonly Dictionary<HttpRequestMessage, byte[]> requestData;

    public RPCClient(string serviceEndpoint)
    {
        this.serviceEndpoint = serviceEndpoint;
        this.requests = new Dictionary<HttpRequestMessage, List<RPCRequest>>();
        this.requestData = new Dictionary<HttpRequestMessage, byte[]>();
    }

    public async Task PostRequest(RPCRequest request, bool async)
    {
        await PostRequests(new List<RPCRequest> { request }, async);
    }

    public async Task PostRequests(List<RPCRequest> requests, bool async)
    {
        foreach (var req in requests)
        {
            var payload = req.Serialize();

            using var httpClient = new HttpClient();
            var serviceRequest = new HttpRequestMessage(HttpMethod.Post, serviceEndpoint)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            try {
                serviceRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                serviceRequest.Headers.TryAddWithoutValidation("User-Agent", "dotNET-JSONRpc/1.0");
                serviceRequest.Content.Headers.ContentLength = payload.Length;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                HandleFailedRequests(requests, new RPCError(RPCErrorCode.InternalError));
            }

            try
            {
                HttpResponseMessage response;
                if (async)
                {
                    response = await httpClient.SendAsync(serviceRequest);
                }
                else
                {
                    response = await httpClient.SendAsync(serviceRequest).ConfigureAwait(false);
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                var responseData = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                HandleData(responseData, requests);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Warning: Network Error - {ex.Message}");
                HandleFailedRequests(requests, new RPCError(RPCErrorCode.NetworkError, "Unable to reach server host"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                HandleFailedRequests(requests, new RPCError(RPCErrorCode.InternalError));
            }
        }
    }

    private void HandleData(byte[] data, List<RPCRequest> requests)
    {
        try
        {
            var results = Encoding.UTF8.GetString(data);

            var json_res = JsonSerializer.Deserialize<object>(results);

            foreach (var request in requests)
            {

                if (request.Callback != null)
                {
                    request.Callback(new RPCResponse {  });
                }
            }
            // Process results and invoke callbacks
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error handling data: {ex.Message}");
            HandleFailedRequests(requests, new RPCError(RPCErrorCode.ParseError, "Received invalid JSON response", Encoding.UTF8.GetString(data)));
        }
    }

    private void HandleFailedRequests(List<RPCRequest> requests, RPCError error)
    {
        foreach (var request in requests)
        {
            if (request.Callback != null) {
                request.Callback(new RPCResponse { Error = error });
            }
        }
    }
}

