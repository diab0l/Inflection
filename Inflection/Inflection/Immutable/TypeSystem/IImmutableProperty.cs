namespace Inflection.Immutable.TypeSystem
{
    using System;

    using Monads;

    using Visitors;

    public interface IImmutableProperty : IImmutableMember
    {
        IImmutableType PropertyType { get; }

        bool HasGetter { get; }

        bool HasWither { get; }

        void Accept(IImmutablePropertyVisitor visitor);

        object GetValue(object obj);
        
        object WithValue(object obj, object value);
    }

    public interface IImmutableProperty<TProperty> : IImmutableProperty
    {
        new IImmutableType<TProperty> PropertyType { get; }

        void Accept(IImmutablePropertyVisitor<TProperty> visitor);
    }

    public interface IImmutablePropertyMember<TDeclaring> : IImmutableProperty, IImmutableMember<TDeclaring>
    {
        void Accept(IImmutablePropertyMemberVisitor<TDeclaring> visitor);
    }

    public interface IImmutableProperty<TDeclaring, TProperty> : IImmutablePropertyMember<TDeclaring>, IImmutableProperty<TProperty>
    {
        Func<TDeclaring, TProperty> Get { get; }

        IMaybe<Func<TDeclaring, TProperty, TDeclaring>> With { get; }
    }
}