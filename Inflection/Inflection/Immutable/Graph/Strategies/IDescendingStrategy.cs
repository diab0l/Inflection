namespace Inflection.Immutable.Graph.Strategies
{
    using System.Collections.Generic;

    public interface IDescendingStrategy<TRoot>
    {
        IEnumerable<ITypeDescendant<TRoot>> GetChildren(ITypeDescendant<TRoot> descendant);
    }
}