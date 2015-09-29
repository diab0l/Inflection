namespace Inflection.TypeExtensions.Extension
{
    using System;

    public static class Transform
    {
        public static Transform<TRoot, TB, TId> Combine<TRoot, TA, TB, TId>(
            this ITransform<TRoot, TA, TId> ta,
            Func<TRoot, TA> getA,
            Func<TA, TB, TA> withB,
            Func<TId, TId> f)
        {
            if (ta == null)
            {
                return null;
            }

            var withC = (getA == null || withB == null) ? null : new Func<TRoot, TB, TRoot>((r, b) => ta.With(r, withB(getA(r), b)));
            var idC = f == null ? default(TId) : f(ta.Id);

            return new Transform<TRoot, TB, TId>(withC, idC);
        }
    }

    public class Transform<TRoot, T, TId> : ITransform<TRoot, T, TId>
    {
        public readonly Func<TRoot, T, TRoot> with;
        public readonly TId id;

        public Transform(Func<TRoot, T, TRoot> with, TId id)
        {
            this.with = with;
            this.id = id;
        }

        public Func<TRoot, T, TRoot> With
        {
            get { return this.with; }
        }

        public TId Id
        {
            get { return this.id; }
        }
    }
}