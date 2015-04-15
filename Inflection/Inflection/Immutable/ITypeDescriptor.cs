namespace Inflection.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface ITypeDescriptor
    {
        Type ClrType { get; }

        IEnumerable<IPropertyDescriptor> GetProperties();
    }

    public interface ITypeDescriptor<TDeclaring> : ITypeDescriptor
    {
        IPropertyDescriptor<TDeclaring, TChild> GetProperty<TChild>(Expression<Func<TDeclaring, TChild>> propExpr);

        new IEnumerable<IPropertyDescriptor<TDeclaring>> GetProperties();

        IEnumerable<IPropertyDescriptor<TDeclaring, TChild>> GetProperties<TChild>();
    }
}
