namespace Inflection.Immutable.Extensions
{
    using System;
    using System.Linq;

    using Immutable.Graph;
    using Immutable.Monads;

    public static class ObjectGraphExtensions
    {
        private static readonly IInflector Inflector = new ReflectingMutableTypeInflector();

        public static IObjectGraph<T> GMap<T, TA, TB>(this IObjectGraph<T> @this, Func<TA, TB> f)
            where TB : TA
        {
            var descendants = @this.GetDescendants<TA>().Select(x => x.Open())
                                   .Where(x => !x.IsEmpty)
                                   .Select(x => x.GetValueOrDefault());

            var root = descendants.Aggregate(@this.Value, (x, y) => y.Set.FMap(z => z(x, f(y.Get(x)))).GetValueOrDefault(x));

            return ObjectGraph.Create(@this.NodeType.Inflector, root);
        }
    }
}