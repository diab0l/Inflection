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
        private readonly Lazy<ImmutableDictionary<MemberInfo, IImmutableProperty<TDeclaring>>> properties;

        public ImmutableType(IEnumerable<IImmutableProperty<TDeclaring>> properties)
        {
            this.properties = new Lazy<ImmutableDictionary<MemberInfo, IImmutableProperty<TDeclaring>>>(
                () => properties.ToImmutableDictionary(x => x.ClrMember));
        }

        public Type ClrType
        {
            get { return typeof(TDeclaring); }
        }

        protected ImmutableDictionary<MemberInfo, IImmutableProperty<TDeclaring>> Properties
        {
            get { return this.properties.Value; }
        }

        IEnumerable<IImmutableProperty> IImmutableType.GetProperties()
        {
            return this.Properties.Values;
        }

        public void Accept(IImmutableTypeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(IImmutableTypePropertiesVisitor visitor)
        {
            visitor.Visit(this.Properties.MaybeGetValue);
        }

        public IMaybe<IImmutableProperty<TDeclaring, TChild>> GetProperty<TChild>(Expression<Func<TDeclaring, TChild>> propExpr)
        {
            var memExpr = propExpr.Body as MemberExpression;
            if (memExpr == null)
            {
                throw new ArgumentException("Expression must be a member access expression", "propExpr");
            }

            var prop = memExpr.Member as PropertyInfo;
            if (prop == null)
            {
                throw new ArgumentException();
            }

            return this.Properties
                       .MaybeGetValue(prop)
                       .Apply(x => Maybe.Return(x as IImmutableProperty<TDeclaring, TChild>));
        }

        public IEnumerable<IImmutableProperty<TDeclaring>> GetProperties()
        {
            return this.Properties.Values;
        }

        public IEnumerable<IImmutableProperty<TDeclaring, TProperty>> GetProperties<TProperty>()
        {
            return this.GetProperties().OfType<IImmutableProperty<TDeclaring, TProperty>>();
        }

        public void Accept(IImmutableTypePropertiesVisitor<TDeclaring> visitor)
        {
            visitor.Visit(this.Properties.MaybeGetValue);
        }
    }
}