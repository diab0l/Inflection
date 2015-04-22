#pragma warning disable 183 // "The given expression is always of the provided type. Consider comparing with 'null' instead."
namespace Inflection.Immutable.Graph.Visitors
{
    using Nodes;

    using TypeSystem;
    using TypeSystem.Visitors;

    public class ObjectDescendantBuilderVisitor<TRoot, TDeclaring> : IImmutablePropertyVisitor<TDeclaring>
    {
        private IObjectDescendant<TRoot, TDeclaring> parent;

        private IObjectDescendant<TRoot> descendant;

        public void Visit<TProperty>(IImmutableProperty<TDeclaring, TProperty> prop)
        {
            var value = prop.Get(this.parent.Value);

            if (!(value is TProperty))
            {
                return;
            }

            this.descendant = ObjectDescendant<TRoot, TProperty>.Create(this.parent, prop, value);
        }

        public IObjectDescendant<TRoot> GetChild(IObjectDescendant<TRoot, TDeclaring> parent, IImmutableProperty<TDeclaring> property)
        {
            this.parent = parent;

            this.descendant = null;
            property.Accept(this);
            return this.descendant;
        }
    }
}
#pragma warning restore 183
