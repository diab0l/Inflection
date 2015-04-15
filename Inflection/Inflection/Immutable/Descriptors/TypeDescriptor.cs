namespace Inflection.Immutable.Descriptors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public abstract class TypeDescriptor : ITypeDescriptor
    {
        private readonly Type clrType;
        private readonly Lazy<ImmutableDictionary<MemberInfo, IPropertyDescriptor>> properties;

        protected TypeDescriptor(Type clrType, IEnumerable<IPropertyDescriptor> properties)
        {
            this.clrType = clrType;
            this.properties = new Lazy<ImmutableDictionary<MemberInfo, IPropertyDescriptor>>(
                () => properties.ToImmutableDictionary(x => x.ClrMember));
        }

        public virtual Type ClrType
        {
            get { return this.clrType; }
        }

        protected ImmutableDictionary<MemberInfo, IPropertyDescriptor> Properties
        {
            get { return this.properties.Value; }
        }

        public IEnumerable<IPropertyDescriptor> GetProperties()
        {
            return this.Properties.Values;
        }
    }

    public class TypeDescriptor<TDeclaring> : TypeDescriptor, ITypeDescriptor<TDeclaring>
    {
        public TypeDescriptor(IEnumerable<IPropertyDescriptor<TDeclaring>> properties) :
            base(typeof(TDeclaring), properties)
        { }

        public IPropertyDescriptor<TDeclaring, TChild> GetProperty<TChild>(Expression<Func<TDeclaring, TChild>> propExpr)
        {
            var memExpr = propExpr.Body as MemberExpression;
            if (memExpr == null)
            {
                throw new ArgumentException();
            }

            var prop = memExpr.Member as PropertyInfo;
            if (prop == null)
            {
                throw new ArgumentException();
            }

            if (!this.Properties.ContainsKey(prop))
            {
                throw new NotImplementedException();
            }

            return this.Properties[prop] as IPropertyDescriptor<TDeclaring, TChild>;
        }

        public new IEnumerable<IPropertyDescriptor<TDeclaring>> GetProperties()
        {
            return base.GetProperties().Cast<IPropertyDescriptor<TDeclaring>>();
        }

        public IEnumerable<IPropertyDescriptor<TDeclaring, TProperty>> GetProperties<TProperty>()
        {
            return this.GetProperties().OfType<IPropertyDescriptor<TDeclaring, TProperty>>();
        }
    }
}