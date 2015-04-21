namespace Inflection.Immutable.Monads
{
    using System;

#pragma warning disable 183 // "The expression is always of the provided type, consider comparing with 'null' instead."
                            // Justification: Abusing the is-operator's semantics is definitely better than 
                            //                possibly comparing a value type with 'null'.
                            //                The is-operator is always true for value typed values and false for reference
                            //                typed values which are null.
    public static class Maybe
    {
        public static IMaybe<TA> Return<TA>(TA value)
        {
            if (value is TA)
            {
                return new Just<TA>(value);
            }

            return new Nothing<TA>();
        }

        public static IMaybe<TB> Bind<TA, TB>(this IMaybe<TA> ma, Func<TA, IMaybe<TB>> f)
        {
            if (ma == null)
            {
                throw new ArgumentNullException();
            }

            return !ma.IsEmpty
                ? f(((Just<TA>)ma).Value)
                : new Nothing<TB>();
        }

        public static IMaybe<TB> FMap<TA, TB>(this IMaybe<TA> ma, Func<TA, TB> f)
        {
            if (ma == null)
            {
                throw new ArgumentNullException();
            }

            return !ma.IsEmpty
                ? (IMaybe<TB>)new Just<TB>(f(((Just<TA>)ma).Value))
                : new Nothing<TB>();
        }

        public static T GetValueOrDefault<T>(this IMaybe<T> @this)
        {
            return @this.IsEmpty
                ? default(T)
                : ((Just<T>)@this).Value;
        }
    }

    public interface IMaybe<out T>
    {
        bool IsEmpty { get; }
    }

    public struct Just<T> : IMaybe<T>
    {
        public readonly T Value;

        public Just(T value)
        {
            if (!(value is T))
            {
                throw new ArgumentNullException("value");
            }

            this.Value = value;
        }

        public bool IsEmpty
        {
            get { return false; }
        }
    }

    public struct Nothing<T> : IMaybe<T>
    {
        public bool IsEmpty
        {
            get { return true; }
        }
    }
}
#pragma warning restore 183
