namespace Inflection.TypeGraph.Graph.Strategies
{
    using System.Collections.Generic;

    public interface IDescendingStrategy<TRoot>
    {
        IEnumerable<ITypeDescendant<TRoot>> GetChildren(ITypeDescendant<TRoot> descendant);

        void Enter(ITypeDescendant<TRoot> descendant);

        void Leave(ITypeDescendant<TRoot> descendant);
    }
}