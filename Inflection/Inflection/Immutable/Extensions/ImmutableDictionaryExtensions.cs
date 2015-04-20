namespace Inflection.Immutable.Extensions
{
    using System;
    using System.Collections.Immutable;

    using Monads;

    public static class ImmutableDictionaryExtensions
    {
        public static IMaybe<TValue> MaybeGetValue<TKey, TValue>(this IImmutableDictionary<TKey, TValue> @this, TKey key)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            TValue value;

            return !@this.TryGetValue(key, out value) 
                ? new Nothing<TValue>()
                : Maybe.Return(value);
        } 
    }
}