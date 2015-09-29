namespace Inflection.Immutable.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    using Monads;

    using Visitors;

    public interface IImmutableType
    {
        IInflector Inflector { get; }

        Type ClrType { get; }

        IEnumerable<IImmutableProperty> GetProperties();

        IMaybe<IImmutableProperty> GetProperty(MemberInfo memberInfo);

        void Accept(IImmutableTypeVisitor visitor);
        
        bool Extends(IImmutableType type);
    }

    public interface IImmutableType<TDeclaring> : IImmutableType
    {
        new IMaybe<IImmutablePropertyMember<TDeclaring>> GetProperty(MemberInfo memberInfo);

        IMaybe<IImmutableProperty<TDeclaring, TChild>> GetProperty<TChild>(Expression<Func<TDeclaring, TChild>> propExpr);

        new IEnumerable<IImmutablePropertyMember<TDeclaring>> GetProperties();

        IEnumerable<IImmutableProperty<TDeclaring, TChild>> GetProperties<TChild>();
    }
}
