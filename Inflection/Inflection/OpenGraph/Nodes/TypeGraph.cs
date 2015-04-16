namespace Inflection.OpenGraph.Nodes
{
    using System.Collections.Generic;

    using global::Inflection.Immutable;
    using global::Inflection.Inflection;

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
        public TypeGraph(ITypeDescriptor<TRoot> rootType, IEnumerable<IPropertyDescriptor<TRoot>> members) 
            : base(rootType, rootType, x => x, (x, y) => y, x => x, (x, y) => y, members)
        { }
    }
}
