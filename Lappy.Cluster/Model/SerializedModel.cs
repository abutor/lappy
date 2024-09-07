using Lappy.Cluster.Helpers;
using System.Reflection;

namespace Lappy.Cluster.Model;

internal class SerializedModel
{
    public string Type { get; set; } = string.Empty;
    public byte[]? Data { get; set; } = null;

    internal static SerializedModel? Create(object? value, Type? type = null)
    {
        type ??= value?.GetType();
        return new SerializedModel
        {
            Type = BodySerializer.GetType(type!),
            Data = type == typeof(CancellationToken) ? null : SpanJson.JsonSerializer.NonGeneric.Utf8.Serialize(value)
        };
    }

    internal object? GetObject()
    {
        if (Data == null || Data.Length == 0)
        {
            return null;
        }

        var type = System.Type.GetType(Type);

        if (type == null)
        {
            return null;
        }

        return SpanJson.JsonSerializer.NonGeneric.Utf8.Deserialize(Data, type);
    }

    internal Type? GetObjectType() => System.Type.GetType(Type);
}
