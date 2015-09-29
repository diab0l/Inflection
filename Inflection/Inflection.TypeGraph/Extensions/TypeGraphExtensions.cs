namespace Inflection.TypeGraph.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Graph;
    using Graph.Strategies;
    using Inflection.Immutable.Monads;

    public static class TypeGraphExtensions
    {
        public static T GMap<T, TA, TB>(this ITypeGraph<T> @this, T root, Func<TA, TB> f)
            where TB : TA
        {
            var strategy = DfsStrategy.Create<T>()
                                      .WithNullCheck(root)
                                      .WithCache()
                                      .WithCycleDetector();

            var descendants = @this.GetDescendants<TA>(strategy);

            return descendants.Aggregate(root, (x, y) => y.Set.FMap(set => set(x, f(y.Get(x)))).GetValueOrDefault(x));
        }

        public static IEnumerable<TB> GSelect<T, TA, TB>(this ITypeGraph<T> @this, T root, Func<TA, TB> f)
        {
            var strategy = DfsStrategy.Create<T>()
                                      .WithNullCheck(root)
                                      .WithCache()
                                      .WithCycleDetector();

            var descendants = @this.GetDescendants<TA>(strategy);

            return descendants.Select(d => f(d.Get(root)));
        }
    }
}
