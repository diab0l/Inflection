namespace Inflection.OpenGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using global::Inflection.Immutable;

    using Visitors;

    public interface ITypeDescendant
    {
        ITypeDescriptor RootType { get; }

        ITypeDescriptor NodeType { get; }

        Expression GetExpression { get; }

        Expression SetExpression { get; }
    }

    public interface ITypeDescendant<TRoot> : ITypeDescendant
    {
        new ITypeDescriptor<TRoot> RootType { get; }

        void Accept(IGraphNodeChildrenVisitor<TRoot> visitor);

        // TODO: create a way to explicitly use DFS or BFS
        IEnumerable<ITypeDescendant<TRoot, T>> GetDescendants<T>();
    }

    public interface ITypeDescendant<TRoot, TNode> : ITypeDescendant<TRoot>
    {
        new ITypeDescriptor<TNode> NodeType { get; }

        new Expression<Func<TRoot, TNode>> GetExpression { get; }

        new Expression<Func<TRoot, TNode, TRoot>> SetExpression { get; }

        Func<TRoot, TNode> Get { get; }

        Func<TRoot, TNode, TRoot> Set { get; }

        IEnumerable<ITypeDescendant<TRoot, T>> GetChildren<T>();

        ITypeDescendant<TRoot, T> GetDescendant<T>(Expression<Func<TNode, T>> propertyExpr);
    }
}