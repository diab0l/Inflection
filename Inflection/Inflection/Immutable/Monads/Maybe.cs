namespace Inflection.Immutable.Monads
{
    using System;

    public static class Maybe
    {
#pragma warning disable 183
        public static IMaybe<T> Return<T>(T value)
        {
            if (value is T)
            {
                return new Just<T>(value);
            }

            return new Nothing<T>();
        }
#pragma warning restore 183

        public static IMaybe<T> Bind<T>(this IMaybe<T> @this, Func<T, IMaybe<T>> op)
        {
            return @this.Apply(op);
        }

        public static IMaybe<T2> Apply<T, T2>(this IMaybe<T> @this, Func<T, IMaybe<T2>> op)
        {
            if (@this == null)
            {
                throw new ArgumentNullException();
            }

            return !@this.IsEmpty
                ? op(((Just<T>)@this).Value)
                : new Nothing<T2>();
        }

        public static IMaybe<T2> Transform<T, T2>(this IMaybe<T> @this, Func<T, T2> op)
        {
            if (@this == null)
            {
                throw new ArgumentNullException();
            }

            return !@this.IsEmpty
                ? (IMaybe<T2>)new Just<T2>(op(((Just<T>)@this).Value))
                : new Nothing<T2>();
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