namespace Inflection.Immutable.Graph.Visitors
{
    using Immutable.Graph;

    public class NullCheckVisitor<T> : ITypeDescendantVisitor<T>
    {
        private readonly T root;
        private bool hasValue;

        public NullCheckVisitor(T root)
        {
            this.root = root;
        }

        public void Visit<TDescendant>(ITypeDescendant<T, TDescendant> typeDescendant)
        {
            this.hasValue = typeDescendant.Get(this.root) is TDescendant;
        }

        public bool HasValue(ITypeDescendant<T> td)
        {
            td.Accept(this);

            return this.hasValue;
        }
    }
}