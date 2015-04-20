namespace Inflection.Immutable.TypeSystem.Visitors
{
    public interface IImmutableTypeVisitor
    {
        void Visit<TDeclaring>(IImmutableType<TDeclaring> type);
    }
}