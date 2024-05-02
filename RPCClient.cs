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
using System.Text.Json.Nodes;

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
        Debug.WriteLine("Received response, handling data");
        try
        {
            var results = Encoding.UTF8.GetString(data);

            var json_res = JsonSerializer.Deserialize<Dictionary<string, object>>(results);

            foreach (var request in requests)
            {
                if (!json_res.TryGetValue("error", out var value))
                {
                    request.Callback?.Invoke(new RPCResponse
                    {
                        Id = json_res["id"].ToString(),
                        Version = json_res["jsonrpc"].ToString(),
                        Result = json_res["result"]
                    });
                }
                else
                {
                    var temp = value.ToString();
                    request.Callback?.Invoke(new RPCResponse
                    {
                        Id = json_res["id"].ToString(),
                        Error = new RPCError(JsonSerializer.Deserialize<Dictionary<string, object>>(temp)),
                        Version = json_res["jsonrpc"].ToString()
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error handling data: {ex.Message}");
            HandleFailedRequests(requests, new RPCError(code: RPCErrorCode.ParseError, ex.Message , data: Encoding.UTF8.GetString(data)));
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

