namespace Inflection.TypeNode.TypeNode {
    using Immutable.TypeSystem;
    using Immutable.TypeSystem.Visitors;

    public class ChildBuilder<TNode> : IImmutablePropertyMemberVisitor<TNode> {
        private ITypeNode child;

        public ITypeNode BuildChild(IImmutablePropertyMember<TNode> visitee) {
            this.child = null;
            visitee.Accept(this);

            return this.child;
        }

        void IImmutablePropertyMemberVisitor<TNode>.Visit<TProperty>(IImmutableProperty<TNode, TProperty> prop) {
            this.child = new TypeNode<TNode, TProperty>(prop.PropertyType, prop.Get, prop.With);
        }
    }
}