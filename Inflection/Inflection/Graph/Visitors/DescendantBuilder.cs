namespace Inflection.Graph.Visitors
{
    using Immutable;

    using Nodes;

    public class DescendantBuilder<TRoot, TNode> : IPropertyDescriptorVisitor<TNode>
    {
        private IDescendant<TRoot, TNode> parent;

        private IDescendant<TRoot> descendant;

        public void Visit<TProperty>(IPropertyDescriptor<TNode, TProperty> prop)
        {
            this.descendant = Descendant.Create(this.parent, prop);
        }

        public IDescendant<TRoot> Build(IDescendant<TRoot, TNode> parent, IPropertyDescriptor<TNode> prop)
        {
            this.parent = parent;

            this.descendant = null;
            prop.Accept(this);

            return this.descendant;
        }
    }
}