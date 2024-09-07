namespace Lappy.Cluster.Model;

internal record ReplyRequest(SerializedModel? Body);
internal record ExceptionRequest(string JsonString, string ExceptionType);
internal record CancelRequest();

internal record LockTenantRequest(string TenantKey, string ServiceQueue, string NodeQueue);
internal record EventRequest(string Key, SerializedModel Model);
internal record InvokeRequest
{
    public string Method { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public string[] ParameterTypes { get; set; } = [];
    public SerializedModel?[] Parameters { get; set; } = [];
};
