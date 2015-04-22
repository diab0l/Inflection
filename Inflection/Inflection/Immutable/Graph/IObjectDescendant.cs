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

        IMaybe<IObjectDescendant> Parent { get; }

        IMaybe<IImmutableProperty> Property { get; } 
    }

    public interface IObjectDescendant<TRoot> : IObjectDescendant
    {
        new IMaybe<IObjectDescendant<TRoot>> Parent { get; }

        IEnumerable<IObjectDescendant<TRoot, TDescendant>> GetChildren<TDescendant>();

        IEnumerable<IObjectDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>();
    }

    public interface IObjectDescendant<TRoot, TNode> : IObjectDescendant<TRoot>
    {
        new IImmutableType<TNode> NodeType { get; }

        TNode Value { get; }

        Expression<Func<TRoot, TNode>> GetExpression { get; }

        IMaybe<Func<TNode, TRoot>> Set { get; }

        IMaybe<IObjectDescendant<TRoot, TDescendant>> GetDescendant<TDescendant>(Expression<Func<TNode, TDescendant>> memExpr);

        IMaybe<ITypeDescendant<TRoot, TNode>> Open();
    }
}
