namespace Inflection.TypeGraph.Graph.Visitors
{
    using Immutable.TypeSystem;
    using Immutable.TypeSystem.Visitors;

    using Nodes;

    public class TypeDescendantBuilderVisitor<TRoot, TNode> : IImmutablePropertyMemberVisitor<TNode>
    {
        private ITypeDescendant<TRoot, TNode> parent;

        private ITypeDescendant<TRoot> typeDescendant;

        public void Visit<TProperty>(IImmutableProperty<TNode, TProperty> prop)
        {
            this.typeDescendant = TypeDescendant.Create(this.parent, prop);
        }

        public ITypeDescendant<TRoot> Build(ITypeDescendant<TRoot, TNode> parent, IImmutablePropertyMember<TNode> prop)
        {
            this.parent = parent;

            this.typeDescendant = null;
            prop.Accept(this);
            return this.typeDescendant;
        }
    }
}