namespace Inflection.Immutable
{
    public interface IPropertyDescriptorVisitor<TDeclaring>
    {
        void Visit<TProperty>(IPropertyDescriptor<TDeclaring, TProperty> prop);
    }
}