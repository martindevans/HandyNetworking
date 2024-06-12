namespace HandyNetworking.Polyfill.Extensions;

public static class DictionaryExtensions
{
#if !NET8_0_OR_GREATER
    public static bool Remove<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue value)
    {
        // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall
        if (!dict.TryGetValue(key, out value))
            return false;

        dict.Remove(key);
        return true;
    }
#endif
}