namespace jsonrpc;


using System;

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

    public Dictionary<string, object> Serialize()
    {
        var payload = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(Version))
        {
            payload["jsonrpc"] = Version;
        }
        if (!string.IsNullOrEmpty(Method))
        {
            payload["method"] = Method;
        }
        if (Params != null)
        {
            payload["params"] = Params;
        }
        if (!string.IsNullOrEmpty(Id))
        {
            payload["id"] = Id;
        }
        return payload;
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
