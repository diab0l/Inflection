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

        //public static IMaybe<TB> Bind<TA, TB>(this IMaybe<TA> ma, Func<TA, IMaybe<TB>> f)
        //{
        //    if (ma == null)
        //    {
        //        throw new ArgumentNullException();
        //    }
            
        //    return !ma.IsEmpty
        //        ? f(((Just<TA>)ma).Value)
        //        : new Nothing<TB>();
        //}

        //public static IMaybe<TB> FMap<TA, TB>(this IMaybe<TA> ma, Func<TA, TB> f)
        //{
        //    if (ma == null)
        //    {
        //        throw new ArgumentNullException();
        //    }

        //    return !ma.IsEmpty
        //        ? Maybe.Return(f(((Just<TA>)ma).Value))
        //        : new Nothing<TB>();
        //}

        public static T GetValueOrDefault<T>(this IMaybe<T> @this, T @default = default(T))
        {
            var result = @default;
            
            @this.FMap(
                       x =>
                       {
                           result = x;
                           return x;
                       });

            return result;
        }
    }

    public interface IMaybe<out T>
    {
        IMaybe<TB> FMap<TB>(Func<T, TB> f);

        IMaybe<TB> Bind<TB>(Func<T, IMaybe<TB>> f);

        T GetValueOrDefault();
    }

    public struct Just<T> : IMaybe<T>
    {
        public readonly T Value;

        public Just(T value)
        {
            this.Value = value;
        }

        public IMaybe<TB> FMap<TB>(Func<T, TB> f)
        {
            return new Just<TB>(f(this.Value));
        }

        public IMaybe<TB> Bind<TB>(Func<T, IMaybe<TB>> f)
        {
            return f(this.Value);
        }

        public T GetValueOrDefault()
        {
            return this.Value;
        }

        public override string ToString()
        {
            return string.Format("Just({0})", this.Value);
        }
    }

    public struct Nothing<T> : IMaybe<T>
    {
        public IMaybe<TB> FMap<TB>(Func<T, TB> f)
        {
            return new Nothing<TB>();
        }

        public IMaybe<TB> Bind<TB>(Func<T, IMaybe<TB>> f)
        {
            return new Nothing<TB>();
        }

        public T GetValueOrDefault()
        {
            return default(T);
        }

        public override string ToString()
        {
            return "Nothing";
        }
    }
}
#pragma warning restore 183
