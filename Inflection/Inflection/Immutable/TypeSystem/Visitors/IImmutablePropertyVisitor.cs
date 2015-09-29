namespace Inflection.Immutable.TypeSystem.Visitors
{
    public interface IImmutablePropertyVisitor
    {
        void Visit<TDeclaring, TProperty>(IImmutableProperty<TDeclaring, TProperty> prop);
    }

    public interface IImmutablePropertyVisitor<TProperty>
    {
        void Visit<TDeclaring>(IImmutableProperty<TDeclaring, TProperty> prop);
    }

    public interface IImmutablePropertyMemberVisitor<TDeclaring>
    {
        void Visit<TProperty>(IImmutableProperty<TDeclaring, TProperty> prop);
    }
}