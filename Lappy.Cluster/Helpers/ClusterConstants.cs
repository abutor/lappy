namespace Lappy.Cluster.Helpers;

internal static class ClusterConstants
{
    public const string ReadQueue = "read";
    public const string AllNodes = "read";
    public const string WriteQueue = "write";

    public const string ChannelKeyService = "CLUSTER_RABBITMQ_CHANNEL";

    public const string RequestBodyKey = "BODY";
    public const string RequestTenantKey = "LOCK_TENANT_KEY";

    public const string InitialServiceKey = "SERVICE";

    public const string QueueHashSetKey = "QUEUES_HASH_SET";
}
