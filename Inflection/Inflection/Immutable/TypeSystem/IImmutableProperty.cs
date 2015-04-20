namespace Inflection.Immutable.TypeSystem
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using Monads;

    using Visitors;

    public interface IImmutableProperty
    {
        MemberInfo ClrMember { get; }

        IImmutableType DeclaringType { get; }

        IImmutableType PropertyType { get; }

        bool CanRead { get; }

        bool CanWrite { get; }

        Expression GetExpression { get; }

        IMaybe<Expression> SetExpression { get; }

        void Accept(IImmutablePropertyVisitor visitor);
    }

    public interface IImmutableProperty<TDeclaring> : IImmutableProperty
    {
        new IImmutableType<TDeclaring> DeclaringType { get; }

        void Accept(IImmutablePropertyVisitor<TDeclaring> visitor);
    }

    public interface IImmutableProperty<TDeclaring, TProperty> : IImmutableProperty<TDeclaring>
    {
        new IImmutableType<TProperty> PropertyType { get; }

        Func<TDeclaring, TProperty> Get { get; }

        IMaybe<Func<TDeclaring, TProperty, TDeclaring>> Set { get; }

        new Expression<Func<TDeclaring, TProperty>> GetExpression { get; }

        new IMaybe<Expression<Func<TDeclaring, TProperty, TDeclaring>>> SetExpression { get; }
    }
}