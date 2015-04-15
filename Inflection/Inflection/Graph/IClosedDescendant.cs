namespace Inflection.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IClosedDescendant<TRoot, TNode>
    {
        Func<TNode> Get { get; }

        Func<TNode, TRoot> Set { get; }

        IClosedDescendant<TRoot, TDescendant> GetDescendant<TDescendant>(Expression<Func<TNode, TDescendant>> memExpr);

        IEnumerable<IClosedDescendant<TRoot, TDescendant>> GetChildren<TDescendant>();

        IEnumerable<IClosedDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>();

        IDescendant<TRoot, TNode> Open();
    }
}
