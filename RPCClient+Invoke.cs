using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jsonrpc;

public partial class RPCClient
{

    public delegate void RPCSuccessCallback(RPCResponse response);
    public delegate void RPCFailedCallback(RPCError error);

    public string Invoke(RPCRequest request, bool async=true)
    {
        _ = PostRequests([request], async: async);
        return request.Id ?? "";
    }

    public string Invoke(string method, object paramsObj, RPCRequestCallback callback, bool async = true)
    {
        RPCRequest request = new RPCRequest
        {
            Method = method,
            Params = paramsObj,
            Callback = callback
        };
        return Invoke(request, async: async);
    }

    public string Invoke(string method, object paramsObj, RPCSuccessCallback successCallback, RPCFailedCallback failedCallback, bool async = true)
    {
        return Invoke(method, paramsObj, (response) =>
        {
            if (response.Error != null)
            {
                failedCallback(response.Error);
            }
            else
            {
                successCallback(response);
            }
        }, async: async);
    }
}
