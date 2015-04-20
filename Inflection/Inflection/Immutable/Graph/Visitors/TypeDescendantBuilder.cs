namespace Inflection.Immutable.Graph.Visitors
{
    using Nodes;

    using TypeSystem;
    using TypeSystem.Visitors;

    public class TypeDescendantBuilder<TRoot, TNode> : IImmutablePropertyVisitor<TNode>
    {
        private ITypeDescendant<TRoot, TNode> parent;

        private ITypeDescendant<TRoot> typeDescendant;

        public void Visit<TProperty>(IImmutableProperty<TNode, TProperty> prop)
        {
            this.typeDescendant = TypeDescendant.Create(this.parent, prop);
        }

        public ITypeDescendant<TRoot> Build(ITypeDescendant<TRoot, TNode> parent, IImmutableProperty<TNode> prop)
        {
            this.parent = parent;

            this.typeDescendant = null;
            prop.Accept(this);

            return this.typeDescendant;
        }
    }
}