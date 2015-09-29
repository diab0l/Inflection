namespace Inflection.TypeGraph.Graph.Strategies
{
    using System;
    using System.Collections.Generic;

    using Immutable.Extensions;
    using Immutable.TypeSystem;

    public static class CycleDetectingAdapter
    {
        public static CycleDetectionAdapter<TRoot> Create<TRoot>()
        {
            return new CycleDetectionAdapter<TRoot>();
        }

        public static CycleDetectionAdapter<TRoot> WithCycleDetector<TRoot>(this IDescendingStrategy<TRoot> adapted)
        {
            return new CycleDetectionAdapter<TRoot>(adapted);
        }
    }

    public class CycleDetectionAdapter<TRoot> : IDescendingStrategy<TRoot>
    {
        private readonly IDescendingStrategy<TRoot> adapted;
        private readonly Dictionary<IImmutableType, int> seenTypes = new Dictionary<IImmutableType, int>();

        public CycleDetectionAdapter() { }

        public CycleDetectionAdapter(IDescendingStrategy<TRoot> adapted) 
            : this()
        {
            this.adapted = adapted;
        }

        public IEnumerable<ITypeDescendant<TRoot>> GetChildren(ITypeDescendant<TRoot> descendant)
        {
            if (this.seenTypes.GetValueOrDefault(descendant.NodeType) > 1)
            {
                throw new Exception("Cyclic graph detected");
            }

            return this.adapted == null ? descendant.GetChildren() : this.adapted.GetChildren(descendant);
        }

        public void Enter(ITypeDescendant<TRoot> descendant)
        {
            this.seenTypes.AddOrUpdate(descendant.NodeType, 1, y => y + 1);
        }

        public void Leave(ITypeDescendant<TRoot> descendant)
        {
            this.seenTypes.AddOrUpdate(descendant.NodeType, 0, y => y - 1);
        }
    }
}