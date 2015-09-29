namespace Inflection.TypeGraph.Graph.Strategies
{
    using System;
    using System.Collections.Generic;

    public static class DescendingStrategy
    {
        public static DescendingStrategy<TRoot> Create<TRoot>(Func<ITypeDescendant<TRoot>, IEnumerable<ITypeDescendant<TRoot>>> getChildren)
        {
            return new DescendingStrategy<TRoot>(getChildren);
        }
    }

    public class DescendingStrategy<TRoot> : IDescendingStrategy<TRoot>
    {
        private readonly Func<ITypeDescendant<TRoot>, IEnumerable<ITypeDescendant<TRoot>>> getChildren;

        public DescendingStrategy(Func<ITypeDescendant<TRoot>, IEnumerable<ITypeDescendant<TRoot>>> getChildren)
        {
            if (getChildren == null)
            {
                throw new ArgumentNullException(nameof(getChildren));
            }

            this.getChildren = getChildren;
        }

        public IEnumerable<ITypeDescendant<TRoot>> GetChildren(ITypeDescendant<TRoot> descendant)
        {
            return this.getChildren(descendant);
        }

        public void Enter(ITypeDescendant<TRoot> descendant) { }

        public void Leave(ITypeDescendant<TRoot> descendant) { }
    }
}