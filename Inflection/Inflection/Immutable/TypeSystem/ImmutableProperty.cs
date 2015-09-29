namespace Inflection.Immutable.TypeSystem
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using Monads;

    using Visitors;

    public class ImmutableProperty<TDeclaring, TProperty> : IImmutableProperty<TDeclaring, TProperty>
    {
        private readonly MemberInfo clrMember;
        private readonly bool hasGetter;
        private readonly bool hasWither;
        private readonly Lazy<IImmutableType<TDeclaring>> declaringType;
        private readonly Lazy<IImmutableType<TProperty>> propertyType;
        private readonly Func<TDeclaring, TProperty> get;
        private readonly IMaybe<Func<TDeclaring, TProperty, TDeclaring>> mWith;
        private readonly Expression<Func<TDeclaring, TProperty>> getExpression;
        private readonly IMaybe<Expression<Func<TDeclaring, TProperty, TDeclaring>>> withExpression;

        public ImmutableProperty(
            MemberInfo memberInfo,
            Lazy<IImmutableType<TDeclaring>> declaringType,
            Lazy<IImmutableType<TProperty>> propertyType,
            Func<TDeclaring, TProperty> get,
            IMaybe<Func<TDeclaring, TProperty, TDeclaring>> mWith, 
            Expression<Func<TDeclaring, TProperty>> getExpression, 
            IMaybe<Expression<Func<TDeclaring, TProperty, TDeclaring>>> withExpression)
        {
            this.clrMember = memberInfo;

            this.declaringType = declaringType;
            this.propertyType = propertyType;

            this.get = get;
            this.mWith = mWith;
            
            this.getExpression = getExpression;
            this.withExpression = withExpression;

            this.hasGetter = get != null;
            this.hasWither = mWith.GetValueOrDefault() != null;
        }

        public MemberInfo ClrMember
        {
            get { return this.clrMember; }
        }

        IImmutableType IImmutableMember.DeclaringType
        {
            get { return this.declaringType.Value; }
        }

        IImmutableType IImmutableProperty.PropertyType
        {
            get { return this.propertyType.Value; }
        }

        public bool HasGetter
        {
            get { return this.hasGetter; }
        }

        public bool HasWither
        {
            get { return this.hasWither; }
        }

        public IImmutableType<TDeclaring> DeclaringType
        {
            get { return this.declaringType.Value; }
        }

        public IImmutableType<TProperty> PropertyType
        {
            get { return this.propertyType.Value; }
        }

        public Func<TDeclaring, TProperty> Get
        {
            get { return this.get; }
        }

        public IMaybe<Func<TDeclaring, TProperty, TDeclaring>> With
        {
            get { return this.mWith; }
        }

        void IImmutableProperty.Accept(IImmutablePropertyVisitor visitor)
        {
            visitor.Visit(this);
        }

        public object GetValue(object obj)
        {
            if (!(obj is TDeclaring))
            {
                throw new ArgumentException(string.Format("Wrong argument type. Expected '{0}'", typeof(TDeclaring)), nameof(obj));
            }

            return this.get((TDeclaring)obj);
        }

        public object WithValue(object obj, object value)
        {
            var with = this.mWith.GetValueOrDefault();
            if (with == null)
            {
                throw new InvalidOperationException(string.Format("Property '{0}' has no accessible wither", this));
            }

            if (!(obj is TDeclaring))
            {
                throw new ArgumentException(string.Format("Wrong argument type. Expected '{0}'", typeof(TDeclaring)), nameof(obj));
            }

            if (!(value is TProperty) && (value != (object)default(TProperty)))
            {
                throw new ArgumentException(string.Format("Wrong argument type. Expected '{0}'", typeof(TProperty)), nameof(value));
            }

            return with((TDeclaring)obj, (TProperty)value);
        }

        void IImmutableProperty<TProperty>.Accept(IImmutablePropertyVisitor<TProperty> visitor)
        {
            visitor.Visit(this);
        }

        void IImmutablePropertyMember<TDeclaring>.Accept(IImmutablePropertyMemberVisitor<TDeclaring> visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return this.ClrMember.ToString();
        }
    }
}