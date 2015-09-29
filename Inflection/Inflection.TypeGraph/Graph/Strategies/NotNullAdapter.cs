namespace Inflection.TypeGraph.Graph.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Visitors;

    public static class NotNullAdapter
    {
        public static NotNullAdapter<TRoot> WithNullCheck<TRoot>(this IDescendingStrategy<TRoot> adapted, TRoot root)
        {
            return new NotNullAdapter<TRoot>(root, adapted);
        } 
    }

    public class NotNullAdapter<TRoot> : IDescendingStrategy<TRoot>
    {
        private readonly IDescendingStrategy<TRoot> adapted; 
        private readonly NullCheckVisitor<TRoot> visitor;
        
        public NotNullAdapter(TRoot root, IDescendingStrategy<TRoot> adapted)
        {
            if (adapted == null)
            {
                throw new ArgumentNullException(nameof(adapted));
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

        public void Enter(ITypeDescendant<TRoot> descendant) { }

        public void Leave(ITypeDescendant<TRoot> descendant) { }
    }
}