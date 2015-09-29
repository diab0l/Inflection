﻿namespace Inflection.TypeGraph.Graph.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CacheAdapter
    {
        public static CacheAdapter<TRoot> WithCache<TRoot>(this IDescendingStrategy<TRoot> adapted)
        {
            return new CacheAdapter<TRoot>(adapted);
        }
    }

    public class CacheAdapter<TRoot> : IDescendingStrategy<TRoot>
    {
        private readonly IDescendingStrategy<TRoot> adapted;
        private readonly Dictionary<ITypeDescendant<TRoot>, ICollection<ITypeDescendant<TRoot>>> cache = new Dictionary<ITypeDescendant<TRoot>, ICollection<ITypeDescendant<TRoot>>>();

        public CacheAdapter(IDescendingStrategy<TRoot> adapted)
        {
            if (adapted == null)
            {
                throw new ArgumentNullException(nameof(adapted));
            }

            this.adapted = adapted;
        }

        public IEnumerable<ITypeDescendant<TRoot>> GetChildren(ITypeDescendant<TRoot> descendant)
        {
            ICollection<ITypeDescendant<TRoot>> entry;

            if (!this.cache.TryGetValue(descendant, out entry))
            {
                entry = this.adapted.GetChildren(descendant).ToList();
                this.cache[descendant] = entry;
            }

            return entry;
        }
        
        public void Enter(ITypeDescendant<TRoot> descendant) { }

        public void Leave(ITypeDescendant<TRoot> descendant) { }
    }
}