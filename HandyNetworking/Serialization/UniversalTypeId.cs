namespace HandyNetworking.Serialization;

internal static class UniversalTypeId<T>
{
    // ReSharper disable once StaticMemberInGenericType
    public static readonly ulong Id;

    static UniversalTypeId()
    {
        var typeName = typeof(T).FullName!;

        // FNV-1a hash
        var hash = 14695981039346656037UL;
        for (var i = 0; i < typeName.Length; i++)
        {
            hash ^= typeName[i];
            hash *= 1099511628211UL;
        }
        Id = hash;
    }
}