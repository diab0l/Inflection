namespace Inflection.TypeExtensions.Extension
{
    using System;

    public static class Query
    {
        public static Query<TType, T2, TId> Combine<TType, T, T2, TId>(this IQuery<TType, T, TId> qa, Func<T, T2> getB, Func<TId, TId> f)
        {
            if (qa == null)
            {
                throw new ArgumentNullException(nameof(qa));
            }

            if (getB == null)
            {
                return null;
            }

            var getC = new Func<TType, T2>(x => getB(qa.Get(x)));
            var cId = f != null ? f(qa.Id) : default(TId);

            return new Query<TType, T2, TId>(getC, cId);
        }
    }

    public class Query<TType, T, TId> : IQuery<TType, T, TId>
    {
        public readonly Func<TType, T> get;
        public readonly TId id;

        public Query(Func<TType, T> get, TId id)
        {
            this.get = get;
            this.id = id;
        }

        public TId Id
        {
            get { return this.id; }
        }

        public Func<TType, T> Get
        {
            get { return this.get; }
        }
    }
}