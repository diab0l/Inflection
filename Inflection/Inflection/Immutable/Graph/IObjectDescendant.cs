namespace Inflection.Immutable.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Monads;

    using TypeSystem;

    public interface IObjectDescendant
    {
        IImmutableType NodeType { get; }
    }

    public interface IObjectDescendant<TRoot> : IObjectDescendant
    {
        IEnumerable<IObjectDescendant<TRoot, TDescendant>> GetChildren<TDescendant>();

        IEnumerable<IObjectDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>();
    }

    public interface IObjectDescendant<TRoot, TNode> : IObjectDescendant<TRoot>
    {
        new IImmutableType<TNode> NodeType { get; }

        TNode Value { get; }

        IMaybe<Func<TNode, TRoot>> Set { get; }

        IMaybe<IObjectDescendant<TRoot, TDescendant>> GetDescendant<TDescendant>(Expression<Func<TNode, TDescendant>> memExpr);

        ITypeDescendant<TRoot, TNode> Open();
    }
}
