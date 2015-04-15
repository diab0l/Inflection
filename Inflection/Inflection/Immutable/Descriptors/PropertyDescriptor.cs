namespace Inflection.Immutable.Descriptors
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public class PropertyDescriptor<TDeclaring, TProperty> : IPropertyDescriptor<TDeclaring, TProperty>
    {
        private readonly MemberInfo clrMember;
        private readonly bool canRead;
        private readonly bool canWrite;
        private readonly Lazy<ITypeDescriptor<TDeclaring>> declaringType;
        private readonly Lazy<ITypeDescriptor<TProperty>> propertyType;
        private readonly Func<TDeclaring, TProperty> get;
        private readonly Func<TDeclaring, TProperty, TDeclaring> set;
        private readonly Expression<Func<TDeclaring, TProperty>> getExpression;
        private readonly Expression<Func<TDeclaring, TProperty, TDeclaring>> setExpression;

        public PropertyDescriptor(
            MemberInfo memberInfo,
            Lazy<ITypeDescriptor<TDeclaring>> declaringType,
            Lazy<ITypeDescriptor<TProperty>> propertyType,
            Func<TDeclaring, TProperty> get,
            Func<TDeclaring, TProperty, TDeclaring> set, 
            Expression<Func<TDeclaring, TProperty>> getExpression, 
            Expression<Func<TDeclaring, TProperty, TDeclaring>> setExpression)
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

        ITypeDescriptor IPropertyDescriptor.DeclaringType
        {
            get { return this.declaringType.Value; }
        }

        ITypeDescriptor IPropertyDescriptor.PropertyType
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

        public ITypeDescriptor<TDeclaring> DeclaringType
        {
            get { return this.declaringType.Value; }
        }

        public ITypeDescriptor<TProperty> PropertyType
        {
            get { return this.propertyType.Value; }
        }

        public Func<TDeclaring, TProperty> Get
        {
            get { return this.get; }
        }

        public Func<TDeclaring, TProperty, TDeclaring> Set
        {
            get { return this.set; }
        }

        Expression IPropertyDescriptor.GetExpression
        {
            get { return this.getExpression; }
        }

        public Expression<Func<TDeclaring, TProperty>> GetExpression
        {
            get { return this.getExpression; }
        }

        Expression IPropertyDescriptor.SetExpression
        {
            get { return this.setExpression; }
        }

        public Expression<Func<TDeclaring, TProperty, TDeclaring>> SetExpression
        {
            get { return this.setExpression; }
        }

        public void Accept(IPropertyDescriptorVisitor<TDeclaring> visitor)
        {
            visitor.Visit(this);
        }
    }
}