using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
            var serializedReq = req.Serialize().ToString()?.ToArray();
            //TODO: Fix maybe null
            var payload = Encoding.UTF8.GetBytes(serializedReq);

            using var httpClient = new HttpClient();
            var serviceRequest = new HttpRequestMessage(HttpMethod.Post, serviceEndpoint)
            {
                Content = new ByteArrayContent(payload)
            };
            serviceRequest.Headers.Add("Content-Type", "application/json");
            serviceRequest.Headers.Add("User-Agent", "dotNET-JSONRpc/1.0");
            serviceRequest.Content.Headers.Add("Content-Length", payload.Length.ToString());

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
                Console.WriteLine($"Warning: Network Error - {ex.Message}");
                HandleFailedRequests(requests, new RPCError(RPCErrorCode.NetworkError));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                HandleFailedRequests(requests, new RPCError(RPCErrorCode.InternalError));
            }
        }
    }

    private void HandleData(byte[] data, List<RPCRequest> requests)
    {
        try
        {
            var results = Encoding.UTF8.GetString(data);

            // Process results and invoke callbacks
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling data: {ex.Message}");
            HandleFailedRequests(requests, new RPCError(RPCErrorCode.ParseError));
        }
    }

    private void HandleFailedRequests(List<RPCRequest> requests, RPCError error)
    {
        foreach (var request in requests)
        {
            //TODO: Fix maybe null
            request.Callback(new RPCResponse { Error = error });
        }
    }
}

