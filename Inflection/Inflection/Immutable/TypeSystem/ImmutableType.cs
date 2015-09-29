namespace Inflection.Immutable.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Extensions;

    using Monads;

    using Visitors;

    public class ImmutableType<TDeclaring> : IImmutableType<TDeclaring>
    {
        private readonly IInflector inflector;

        private readonly Lazy<ImmutableDictionary<MemberInfo, IImmutablePropertyMember<TDeclaring>>> propertyLookup;

        public ImmutableType(IInflector inflector, IEnumerable<IImmutablePropertyMember<TDeclaring>> properties)
        {
            this.inflector = inflector;
            this.propertyLookup = new Lazy<ImmutableDictionary<MemberInfo, IImmutablePropertyMember<TDeclaring>>>(
                () => properties.ToImmutableDictionary(x => x.ClrMember));
        }

        public IInflector Inflector
        {
            get { return this.inflector; }
        }

        public Type ClrType
        {
            get { return typeof(TDeclaring); }
        }

        IEnumerable<IImmutableProperty> IImmutableType.GetProperties()
        {
            return this.propertyLookup.Value.Values;
        }

        public void Accept(IImmutableTypeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public bool Extends(IImmutableType type)
        {
            return this.ClrType.IsAssignableFrom(type.ClrType);
        }
        
        IMaybe<IImmutableProperty> IImmutableType.GetProperty(MemberInfo memberInfo)
        {
            return this.propertyLookup.Value.MaybeGetValue(memberInfo);
        }

        public IMaybe<IImmutablePropertyMember<TDeclaring>> GetProperty(MemberInfo memberInfo)
        {
            return this.propertyLookup.Value.MaybeGetValue(memberInfo);
        }

        public IMaybe<IImmutableProperty<TDeclaring, TChild>> GetProperty<TChild>(Expression<Func<TDeclaring, TChild>> propExpr)
        {
            var memExpr = propExpr.Body as MemberExpression;
            if (memExpr == null)
            {
                throw new ArgumentException("Expression must be a member access expression", nameof(propExpr));
            }

            var prop = memExpr.Member as PropertyInfo;
            if (prop == null)
            {
                throw new ArgumentException();
            }

            return this.propertyLookup
                       .Value
                       .MaybeGetValue(prop)
                       .Bind(x => Maybe.Return(x as IImmutableProperty<TDeclaring, TChild>));
        }

        public IEnumerable<IImmutablePropertyMember<TDeclaring>> GetProperties()
        {
            return this.propertyLookup.Value.Values;
        }

        public IEnumerable<IImmutableProperty<TDeclaring, TProperty>> GetProperties<TProperty>()
        {
            return this.GetProperties().OfType<IImmutableProperty<TDeclaring, TProperty>>();
        }

        public override string ToString()
        {
            return this.ClrType.ToString();
        }
    }
}