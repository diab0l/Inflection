namespace Inflection.TypeNode.TypeNode
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Immutable.Monads;
    using Immutable.TypeSystem;

    public interface ITypeNode
    {
        IImmutableType NodeType { get; }
        
        IMaybe<ITypeNode> MaybeGetChild(MemberInfo memberInfo); 

        IEnumerable<ITypeNode> GetChildren();

        IEnumerable<ITypeNode<TNode>> GetChildren<TNode>();

        IMaybe<ITypeNode> MaybeGetNode(IEnumerable<MemberInfo> memberInfos);
        
        IEnumerable<ITypeNode> GetNodes();

        IEnumerable<ITypeNode<TNode>> GetNodes<TNode>();

        void Accept(ITypeNodeVisitor visitor);
    }

    public interface ITypeNode<TNode> : ITypeNode
    {
        new IImmutableType<TNode> NodeType { get; }

        new IEnumerable<ITypeNode<TNode, T>> GetChildren<T>();

        IMaybe<ITypePath<TNode, T>> MaybeGetPath<T>(IEnumerable<MemberInfo> memberInfos);

        IEnumerable<ITypePath<TNode, T>> GetPaths<T>();
    
        IEnumerable<IValuePath<TNode, T>> GetValuePaths<T>(TNode node);

        IEnumerable<T> GetValues<T>(TNode node);
    }

    public interface ITypeNode<TParent, TNode> : ITypeNode<TNode>
    {
        ITypePath<TParent, TNode> Path { get; }
        
        Func<TParent, TNode> Get { get; }

        IMaybe<Func<TParent, TNode, TParent>> Set { get; }
    }
}