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
        private readonly bool canRead;
        private readonly bool canWrite;
        private readonly Lazy<IImmutableType<TDeclaring>> declaringType;
        private readonly Lazy<IImmutableType<TProperty>> propertyType;
        private readonly Func<TDeclaring, TProperty> get;
        private readonly IMaybe<Func<TDeclaring, TProperty, TDeclaring>> set;
        private readonly Expression<Func<TDeclaring, TProperty>> getExpression;
        private readonly IMaybe<Expression<Func<TDeclaring, TProperty, TDeclaring>>> setExpression;

        public ImmutableProperty(
            MemberInfo memberInfo,
            Lazy<IImmutableType<TDeclaring>> declaringType,
            Lazy<IImmutableType<TProperty>> propertyType,
            Func<TDeclaring, TProperty> get,
            IMaybe<Func<TDeclaring, TProperty, TDeclaring>> set, 
            Expression<Func<TDeclaring, TProperty>> getExpression, 
            IMaybe<Expression<Func<TDeclaring, TProperty, TDeclaring>>> setExpression)
        {
            this.clrMember = memberInfo;

            this.declaringType = declaringType;
            this.propertyType = propertyType;

            this.get = get;
            this.set = set;
            
            this.getExpression = getExpression;
            this.setExpression = setExpression;

            this.canRead = get != null;
            this.canWrite = set != null;
        }

        public MemberInfo ClrMember
        {
            get { return this.clrMember; }
        }

        IImmutableType IImmutableProperty.DeclaringType
        {
            get { return this.declaringType.Value; }
        }

        IImmutableType IImmutableProperty.PropertyType
        {
            get { return this.propertyType.Value; }
        }

        public bool CanRead
        {
            get { return this.canRead; }
        }

        public bool CanWrite
        {
            get { return this.canWrite; }
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

        public IMaybe<Func<TDeclaring, TProperty, TDeclaring>> Set
        {
            get { return this.set; }
        }

        Expression IImmutableProperty.GetExpression
        {
            get { return this.getExpression; }
        }

        public Expression<Func<TDeclaring, TProperty>> GetExpression
        {
            get { return this.getExpression; }
        }

        IMaybe<Expression> IImmutableProperty.SetExpression
        {
            get { return this.setExpression; }
        }

        void IImmutableProperty.Accept(IImmutablePropertyVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IMaybe<Expression<Func<TDeclaring, TProperty, TDeclaring>>> SetExpression
        {
            get { return this.setExpression; }
        }

        public void Accept(IImmutablePropertyVisitor<TDeclaring> visitor)
        {
            visitor.Visit(this);
        }
    }
}