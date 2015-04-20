namespace Inflection.Immutable.Graph
{
    using System;

    using Monads;

    using Nodes;

    using TypeSystem;

    public static class ObjectGraph
    {
        public static ObjectGraph<TRoot> Create<TRoot>(IInflector inflector, TRoot value)
        {
            var typeRoot = TypeGraph.Create<TRoot>(inflector);

            return new ObjectGraph<TRoot>(typeRoot, value);
        }
    }

    public class ObjectGraph<TRoot> : ObjectDescendant<TRoot, TRoot>, IObjectGraph<TRoot>
    {
        public ObjectGraph(ITypeDescendant<TRoot, TRoot> typeDescendant, TRoot value) 
            : base(new Nothing<IObjectDescendant<TRoot>>(), new Nothing<IImmutableProperty>(), typeDescendant.NodeType, Maybe.Return<Func<TRoot, TRoot>>(x => x), value)
        {
        }
    }
}