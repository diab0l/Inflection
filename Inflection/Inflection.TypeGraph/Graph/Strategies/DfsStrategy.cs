namespace Inflection.TypeGraph.Graph.Strategies
{
    using System.Collections.Generic;

    public static class DfsStrategy
    {
        public static DfsStrategy<TRoot> Create<TRoot>()
        {
            return new DfsStrategy<TRoot>();
        }
    }

    public class DfsStrategy<TRoot> : IDescendingStrategy<TRoot>
    {
        public IEnumerable<ITypeDescendant<TRoot>> GetChildren(ITypeDescendant<TRoot> descendant)
        {
            return descendant.GetChildren();
        }

        public void Enter(ITypeDescendant<TRoot> descendant) { }

        public void Leave(ITypeDescendant<TRoot> descendant) { }
    }
}