namespace Inflection.Immutable.Graph
{
    using System;
    using System.Linq.Expressions;

    using Monads;

    using Nodes;

    using TypeSystem;

    public static class TypeGraph
    {
        public static TypeGraph<TRoot> Create<TRoot>(IInflector inflector)
        {
            var rootType = inflector.Inflect<TRoot>();

            return new TypeGraph<TRoot>(rootType);
        }
    }

    public class TypeGraph<TRoot> : TypeDescendant<TRoot, TRoot>, ITypeGraph<TRoot>
    {
        public TypeGraph(IImmutableType<TRoot> rootType) 
            : base(rootType, rootType, x => x, FnToMaybe((x, y) => y), x => x, ExprToMaybe((x, y) => y))
        { }

        private static IMaybe<Func<TRoot, TRoot, TRoot>> FnToMaybe(Func<TRoot, TRoot, TRoot> f)
        {
            return Maybe.Return(f);
        }

        private static IMaybe<Expression<Func<TRoot, TRoot, TRoot>>> ExprToMaybe(Expression<Func<TRoot, TRoot, TRoot>> f)
        {
            return Maybe.Return(f);
        }
    }
}
