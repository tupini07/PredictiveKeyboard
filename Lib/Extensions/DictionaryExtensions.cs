namespace Lib.Extensions
{
    public static class DictionaryExtensions
    {
        /**
         * Taken from https://stackoverflow.com/a/47081934/2234619
         */
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            TValue value;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            if (!dictionary.TryGetValue(key, out value))
            {
                value = valueFactory.Invoke(key);
                dictionary.Add(key, value);
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            return value;
        }
    }
}
