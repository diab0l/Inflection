namespace Inflection.TypeGraph.Graph
{
    using System;
    using System.Linq.Expressions;

    using Immutable.Monads;
    using Immutable.TypeSystem;

    using Nodes;

    public static class TypeGraph
    {
        public static TypeGraph<TRoot> Create<TRoot>(IInflector inflector)
        {
            var rootType = inflector.Inflect<TRoot>();

            return new TypeGraph<TRoot>(rootType);
        }
    }

    public class TypeGraph<TRoot> : TypeDescendant<TRoot, TRoot, TRoot>, ITypeGraph<TRoot>
    {
        public TypeGraph(IImmutableType<TRoot> rootType, bool isMemoizing = true)
            : base(isMemoizing, rootType, rootType, x => x, "x => x", FnToMaybe((x, y) => y))
        { }

        private static IMaybe<Func<TRoot, TRoot, TRoot>> FnToMaybe(Func<TRoot, TRoot, TRoot> f)
        {
            return Maybe.Return(f);
        }

        private static Lazy<IMaybe<Expression<Func<TRoot, TRoot, TRoot>>>> ExprToMaybe(Expression<Func<TRoot, TRoot, TRoot>> f)
        {
            return new Lazy<IMaybe<Expression<Func<TRoot, TRoot, TRoot>>>>(() => Maybe.Return(f));
        }
    }
}
