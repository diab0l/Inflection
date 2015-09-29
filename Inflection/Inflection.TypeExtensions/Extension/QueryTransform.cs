namespace Inflection.TypeExtensions.Extension
{
    using System;

    using Immutable.Monads;

    public static class QueryTransform
    {
        public static QueryTransform<TRoot, TB, TId> Combine<TRoot, TA, TB, TId>(
            this IQueryTransform<TRoot, TA, TId> qta,
            Func<TA, TB> getB,
            Func<TA, TB, TA> withB,
            Func<TId, TId> f)
        {
            if (qta == null)
            {
                throw new ArgumentNullException(nameof(qta));
            }

            var cId = f == null ? default(TId) : f(qta.Id);

            var getA = qta.Get;
            var getC = new Func<TRoot, TB>(x => getB(qta.Get(x)));

            var withA = qta.GetValueOrDefault();
            var tc = (getB == null || withB == null || withA == null) ? null : new Func<TRoot, TB, TRoot>((r, b) => withA(r, withB(getA(r), b)));

            return new QueryTransform<TRoot, TB, TId>(cId, getC, tc);
        }
    }

    public class QueryTransform<TRoot, T, TId> : Query<TRoot, T, TId>, IQueryTransform<TRoot, T, TId>
    {
        public readonly Func<TRoot, T, TRoot> t; 

        public QueryTransform(TId id, Func<TRoot, T> get, Func<TRoot, T, TRoot> t) 
            : base(get, id)
        {
            this.t = t;
        }

        IMaybe<TB> IMaybe<Func<TRoot, T, TRoot>>.FMap<TB>(Func<Func<TRoot, T, TRoot>, TB> f)
        {
            if (this.t == null)
            {
                return new Nothing<TB>();
            }

            return new Just<TB>(f(this.t));
        }

        IMaybe<TB> IMaybe<Func<TRoot, T, TRoot>>.Bind<TB>(Func<Func<TRoot, T, TRoot>, IMaybe<TB>> f)
        {
            if (this.t == null)
            {
                return new Nothing<TB>();
            }

            return f(this.t);
        }

        Func<TRoot, T, TRoot> IMaybe<Func<TRoot, T, TRoot>>.GetValueOrDefault(Func<TRoot, T, TRoot> @default)
        {
            return this.t;
        }
    }
}