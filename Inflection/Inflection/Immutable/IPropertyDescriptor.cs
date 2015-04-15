namespace Inflection.Immutable
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public interface IPropertyDescriptor
    {
        MemberInfo ClrMember { get; }

        ITypeDescriptor DeclaringType { get; }

        ITypeDescriptor PropertyType { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        Expression GetExpression { get; }

        Expression SetExpression { get; }
    }

    public interface IPropertyDescriptor<TDeclaring> : IPropertyDescriptor
    {
        new ITypeDescriptor<TDeclaring> DeclaringType { get; }

        void Accept(IPropertyDescriptorVisitor<TDeclaring> visitor);
    }

    public interface IPropertyDescriptor<TDeclaring, TProperty> : IPropertyDescriptor<TDeclaring>
    {
        new ITypeDescriptor<TProperty> PropertyType { get; }

        Func<TDeclaring, TProperty> Get { get; }

        Func<TDeclaring, TProperty, TDeclaring> Set { get; }

        new Expression<Func<TDeclaring, TProperty>> GetExpression { get; }

        new Expression<Func<TDeclaring, TProperty, TDeclaring>> SetExpression { get; }
    }
}