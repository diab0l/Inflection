namespace Inflection.Immutable.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Monads;

    using Nodes;

    using TypeSystem;

    public static class TypeGraph
    {
        public static TypeGraph<TRoot> Create<TRoot>(IInflector inflector)
        {
            var rootType = inflector.Inflect<TRoot>();
            var members = rootType.GetProperties();

            return new TypeGraph<TRoot>(rootType, members);
        }
    }

    public class TypeGraph<TRoot> : TypeDescendant<TRoot, TRoot>, ITypeGraph<TRoot>
    {
        public TypeGraph(IImmutableType<TRoot> rootType, IEnumerable<IImmutableProperty<TRoot>> members) 
            : base(rootType, rootType, x => x, FnToMaybe((x, y) => y), x => x, ExprToMaybe((x, y) => y), members)
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
