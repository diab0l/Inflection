namespace Inflection.Immutable.Extensions
{
    using System;
    using System.Linq;

    using Immutable.Graph;
    using Immutable.Graph.Strategies;
    using Immutable.Monads;

    public static class TypeGraphExtensions
    {
        public static T GMap<T, TA, TB>(this ITypeGraph<T> @this, T root, Func<TA, TB> f)
            where TB : TA
        {
            var descendants = @this.GetDescendants<TA>(CacheAdapter.Create(new DefaultStrategy<T>(root)));

            return descendants.Aggregate(root, (x, y) => y.Set.FMap(set => set(x, f(y.Get(x)))).GetValueOrDefault(x));
        }
    }
}