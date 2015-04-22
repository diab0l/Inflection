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
            return new ObjectGraph<TRoot>(inflector, value);
        }
    }

    public class ObjectGraph<TRoot> : ObjectDescendant<TRoot, TRoot>, IObjectGraph<TRoot>
    {
        public ObjectGraph(IInflector inflector, TRoot value) 
            : base(new Nothing<IObjectDescendant<TRoot>>(), new Nothing<IImmutableProperty>(), inflector.Inflect<TRoot>(), Maybe.Return<Func<TRoot, TRoot>>(x => x), value, x => x)
        {
        }

        public new ITypeGraph<TRoot> Open()
        {
            return TypeGraph.Create<TRoot>(this.NodeType.Inflector);
        }
    }
}