namespace jsonrpc;

using System;

public class RPCResponse
{
    public string? Version
    {
        get; set;
    }
    public string? Id
    {
        get; set;
    }
    public RPCError? Error
    {
        get; set;
    }
    public object? Result
    {
        get; set;
    }

    public RPCResponse()
    {
        Version = null;
        Id = null;
        Error = null;
        Result = null;
    }

    public RPCResponse(object result, string id, string version)
    {
        Version = version;
        Id = id;
        Result = result;
    }

    public static RPCResponse ResponseWithError(RPCError error)
    {
        return new RPCResponse { Error = error };
    }

    public override string ToString()
    {
        return $"RPCResponse: Result={Result}, Id={Id}, Version={Version}, Error={Error}";
    }

    ~RPCResponse()
    {
        Version = null;
        Id = null;
        Error = null;
        Result = null;
    }
}

