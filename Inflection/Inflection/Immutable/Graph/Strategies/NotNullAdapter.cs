namespace Inflection.Immutable.Graph.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Visitors;

    public class NotNullAdapter<TRoot> : IDescendingStrategy<TRoot>
    {
        private readonly IDescendingStrategy<TRoot> adapted; 
        private readonly NullCheckVisitor<TRoot> visitor;
        
        public NotNullAdapter(TRoot root, IDescendingStrategy<TRoot> adapted)
        {
            if (adapted == null)
            {
                throw new ArgumentNullException("adapted");
            }

            this.adapted = adapted;
            this.visitor = new NullCheckVisitor<TRoot>(root);
        }

        public IEnumerable<ITypeDescendant<TRoot>> GetChildren(ITypeDescendant<TRoot> descendant)
        {
            if (!this.visitor.HasValue(descendant))
            {
                return Enumerable.Empty<ITypeDescendant<TRoot>>();
            }

            return this.adapted.GetChildren(descendant);
        }
    }
}