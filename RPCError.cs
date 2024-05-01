namespace jsonrpc;

using System;

public enum RPCErrorCode
{
    ParseError = -32700,
    InvalidRequest = -32600,
    MethodNotFound = -32601,
    InvalidParams = -32602,
    InternalError = -32603,
    ServerError = 32000,
    NetworkError = 32001
}

public class RPCError
{
    public RPCErrorCode Code
    {
        get; private set;
    }
    public string? Message
    {
        get; private set;
    }
    public object? Data
    {
        get; private set;
    }

    public RPCError(RPCErrorCode code, string message, object? data = null)
    {
        Code = code;
        Message = message;
        Data = data;
    }

    public RPCError() : this(RPCErrorCode.ServerError, "Server error")
    {
    }

    public RPCError(RPCErrorCode code) : this(code, GetDefaultErrorMessage(code))
    {
    }

    public RPCError(Dictionary<string, object> errorDict)
    {
        var errorCode = Convert.ToInt32(value: errorDict["code"]);
        var errorMessage = Convert.ToString(errorDict["message"]);
        var errorData = errorDict.TryGetValue("data", out var value) ? value : null;
        Code = (RPCErrorCode)errorCode;
        Message = errorMessage;
        Data = errorData;
    }

    public override string ToString()
    {
        if (Data != null)
        {
            return $"RPCError: {Message} ({Code}): {Data}.";
        }
        else
        {
            return $"RPCError: {Message} ({Code}).";
        }
    }

    public static RPCError ErrorWithCode(RPCErrorCode code)
    {
        return new RPCError(code);
    }

    public static RPCError? ErrorWithDictionary(Dictionary<string, object> dictionary)
    {
        if (dictionary.TryGetValue("code", out var codeObj) && dictionary.TryGetValue("message", out var messageObj))
        {
            var code = Convert.ToInt32(codeObj);
            var message = Convert.ToString(messageObj);
            object? data;
            if (dictionary.TryGetValue("data", out var value))
            {
                data = value as object;
            }
            else
            {
                data = null;
            }

            return new RPCError((RPCErrorCode)code, message ?? "", data);
        }
        else
        {
            return null;
        }
    }

    private static string GetDefaultErrorMessage(RPCErrorCode code)
    {
        switch (code)
        {
            case RPCErrorCode.ParseError:
                return "Parse error";
            case RPCErrorCode.InvalidRequest:
                return "Invalid Request";
            case RPCErrorCode.MethodNotFound:
                return "Method not found";
            case RPCErrorCode.InvalidParams:
                return "Invalid params";
            case RPCErrorCode.InternalError:
                return "Internal error";
            case RPCErrorCode.NetworkError:
                return "Network error";
            default:
                return "Server error";
        }
    }
}

