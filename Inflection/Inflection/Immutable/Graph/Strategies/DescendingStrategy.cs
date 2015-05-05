namespace Inflection.Immutable.Graph.Strategies
{
    using System;
    using System.Collections.Generic;

    public class DescendingStrategy<TRoot> : IDescendingStrategy<TRoot>
    {
        private readonly Func<ITypeDescendant<TRoot>, IEnumerable<ITypeDescendant<TRoot>>> getChildren;

        public DescendingStrategy(Func<ITypeDescendant<TRoot>, IEnumerable<ITypeDescendant<TRoot>>> getChildren)
        {
            if (getChildren == null)
            {
                throw new ArgumentNullException("getChildren");
            }

            this.getChildren = getChildren;
        }

        public virtual IEnumerable<ITypeDescendant<TRoot>> GetChildren(ITypeDescendant<TRoot> descendant)
        {
            return this.getChildren(descendant);
        }
    }
}