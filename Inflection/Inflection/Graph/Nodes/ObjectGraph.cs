namespace Inflection.Graph.Nodes
{
    using System.Collections.Generic;

    using Immutable;

    using Inflection;

    public static class ObjectGraph
    {
        public static ObjectGraph<TRoot> Create<TRoot>(IInflector inflector)
        {
            var rootType = inflector.Inflect<TRoot>();
            var members = rootType.GetProperties();

            return new ObjectGraph<TRoot>(rootType, members);
        }
    }

    public class ObjectGraph<TRoot> : Descendant<TRoot, TRoot>, IGraphRoot<TRoot>
    {
        public ObjectGraph(ITypeDescriptor<TRoot> rootType, IEnumerable<IPropertyDescriptor<TRoot>> members) 
            : base(rootType, rootType, x => x, (x, y) => y, x => x, (x, y) => y, members)
        { }
    }
}
