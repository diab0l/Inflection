namespace Inflection.Immutable.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Monads;

    using Strategies;

    using TypeSystem;

    using Visitors;

    public interface ITypeDescendant
    {
        bool IsMemoizing { get; }

        IImmutableType RootType { get; }

        IImmutableType NodeType { get; }

        Expression GetExpression { get; }

        IMaybe<Expression> SetExpression { get; }

        IEnumerable<ITypeDescendant> GetChildren();

        void Accept(ITypeDescendantVisitor visitor);

        void Accept(ITypeDescendantChildrenVisitor visitor);
    }

    public interface ITypeDescendant<TRoot> : ITypeDescendant
    {
        new IImmutableType<TRoot> RootType { get; }

        void Accept(ITypeDescendantVisitor<TRoot> visitor);

        void Accept(ITypeDescendantChildrenVisitor<TRoot> visitor);

        new IEnumerable<ITypeDescendant<TRoot>> GetChildren();

        // TODO: create a way to explicitly use DFS or BFS
        IEnumerable<ITypeDescendant<TRoot, T>> GetDescendants<T>();

        IEnumerable<ITypeDescendant<TRoot, T>> GetDescendants<T>(IDescendingStrategy<TRoot> descendingStrategy);
    }

    public interface ITypeDescendant<TRoot, TNode> : ITypeDescendant<TRoot>
    {
        new IImmutableType<TNode> NodeType { get; }

        new Expression<Func<TRoot, TNode>> GetExpression { get; }

        new IMaybe<Expression<Func<TRoot, TNode, TRoot>>> SetExpression { get; }

        Func<TRoot, TNode> Get { get; }

        IMaybe<Func<TRoot, TNode, TRoot>> Set { get; }

        IMaybe<ITypeDescendant<TRoot, T>> GetChild<T>(Expression<Func<TNode, T>> propertyExpr);
        
        IEnumerable<ITypeDescendant<TRoot, T>> GetChildren<T>();

        IMaybe<ITypeDescendant<TRoot, T>> GetDescendant<T>(Expression<Func<TNode, T>> propertyExpr);

        IMaybe<IObjectDescendant<TRoot, TNode>> Close(TRoot value);
    }
}