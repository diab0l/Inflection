namespace Inflection.TypeNode.TypeNode
{
    public interface ITypeNodeVisitor
    {
        void Visit<TParent, TNode>(ITypeNode<TParent, TNode> visitee);
    }
}