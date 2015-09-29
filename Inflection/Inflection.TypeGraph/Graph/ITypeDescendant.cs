namespace Inflection.TypeGraph.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    using Immutable.Monads;
    using Immutable.TypeSystem;

    using Strategies;

    using Visitors;

    public interface ITypeDescendant
    {
        bool IsMemoizing { get; }

        IImmutableType RootType { get; }

        IImmutableType NodeType { get; }

        IEnumerable<ITypeDescendant> GetChildren();

        void Accept(ITypeDescendantVisitor visitor);
    }

    public interface ITypeDescendant<TRoot> : ITypeDescendant
    {
        new IImmutableType<TRoot> RootType { get; }

        IMaybe<ITypeDescendant<TRoot>> MaybeGetChild(MemberInfo memberInfo); 

        new IEnumerable<ITypeDescendant<TRoot>> GetChildren();

        IEnumerable<ITypeDescendant<TRoot, T>> GetDescendants<T>();
        
        IEnumerable<ITypeDescendant<TRoot, T>> GetDescendants<T>(TRoot root);
        
        IEnumerable<ITypeDescendant<TRoot, T>> GetDescendants<T>(IDescendingStrategy<TRoot> descendingStrategy);

        void Accept(ITypeDescendantVisitor<TRoot> visitor);
    }

    public interface ITypeDescendant<TRoot, TNode> : ITypeDescendant<TRoot>
    {
        new IImmutableType<TNode> NodeType { get; }

        Func<TRoot, TNode> Get { get; }

        string GetPath { get; }

        IMaybe<Func<TRoot, TNode, TRoot>> Set { get; }

        TRoot Update(TRoot root, Func<TNode, TNode> f);
        
        IEnumerable<ITypeDescendant<TRoot, T>> GetChildren<T>();

        IMaybe<ITypeDescendant<TRoot, T>> MaybeGetChild<T>(Expression<Func<TNode, T>> propertyExpr);

        IMaybe<ITypeDescendant<TRoot, T>> MaybeGetDescendant<T>(Expression<Func<TNode, T>> propertyExpr);
    }
}