namespace Inflection.Immutable.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue @default = default(TValue))
        {
            TValue value;

            return !@this.TryGetValue(key, out value) ? @default : value;
        }

        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue add, Func<TValue, TValue> update)
        {
            TValue value;

            if (@this.TryGetValue(key, out value))
            {
                @this[key] = update(value);
            }
            else
            {
                @this[key] = add;
            }
        }
    }
}