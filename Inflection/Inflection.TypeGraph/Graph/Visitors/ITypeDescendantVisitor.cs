namespace Inflection.TypeGraph.Graph.Visitors
{
    public interface ITypeDescendantVisitor
    {
        void Visit<TRoot, TDescendant>(ITypeDescendant<TRoot, TDescendant> typeDescendant);
    }

    public interface ITypeDescendantVisitor<TRoot>
    {
        void Visit<TDescendant>(ITypeDescendant<TRoot, TDescendant> typeDescendant);
    }
}