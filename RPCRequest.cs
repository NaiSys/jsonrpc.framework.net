namespace jsonrpc;


using System;
using System.Text.Json;

public delegate void RPCRequestCallback(RPCResponse response);

public class RPCRequest
{
    public string? Version
    {
        get; private set;
    }
    public string? Id
    {
        get; private set;
    }
    public string? Method
    {
        get; set;
    }
    public object? Params
    {
        get; set;
    }
    public RPCRequestCallback? Callback
    {
        get; set;
    }

    public RPCRequest()
    {
        Version = "2.0";
        Id = $"{new Random().Next()}";
        Method = null;
        Params = null;
        Callback = null;
    }

    public static RPCRequest RequestWithMethod(string method)
    {
        var request = new RPCRequest();
        request.Method = method;
        return request;
    }

    public static RPCRequest RequestWithMethod(string method, object parameters)
    {
        var request = RequestWithMethod(method);
        request.Params = parameters;
        return request;
    }

    public static RPCRequest RequestWithMethod(string method, object parameters, RPCRequestCallback callback)
    {
        var request = RequestWithMethod(method, parameters);
        request.Callback = callback;
        return request;
    }

    public override string ToString()
    {
        return $"RPCRequest: Method={Method}, Params={Params}, Id={Id}";
    }

    public string Serialize()
    {
        var payload = new { 
            jsonrpc = Version,
            method = Method,
            id = Id,
            @params = Params
        };
        var v = JsonSerializer.Serialize(payload);
        return v;
    }

    ~RPCRequest()
    {
        Version = null;
        Id = null;
        Method = null;
        Params = null;
        Callback = null;
    }
}
