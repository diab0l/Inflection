namespace Inflection.Inflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Immutable;
    using Immutable.Descriptors;

    //// ReSharper disable AccessToModifiedClosure
    public class ReflectingMutableTypeInflector : InflectorBase
    {
        private readonly Dictionary<Type, ITypeDescriptor> cachedDescriptors = new Dictionary<Type, ITypeDescriptor>();

        public override ITypeDescriptor<TDeclaring> Inflect<TDeclaring>()
        {
            if (this.cachedDescriptors.ContainsKey(typeof(TDeclaring)))
            {
                return this.cachedDescriptors[typeof(TDeclaring)] as ITypeDescriptor<TDeclaring>;
            }

            TypeDescriptor<TDeclaring> desc = null;

            var tdLazy = new Lazy<ITypeDescriptor<TDeclaring>>(() => desc);

            var props = typeof(TDeclaring).GetProperties().Select(p => this.DescribeProperty(p, tdLazy));

            desc = new TypeDescriptor<TDeclaring>(props);

            this.cachedDescriptors[typeof(TDeclaring)] = desc;

            return desc;
        }

        public override ITypeDescriptor Inflect(Type tDeclaring)
        {
            var method = typeof(ReflectingMutableTypeInflector).GetMethod("Describe", Type.EmptyTypes);

            return method.MakeGenericMethod(tDeclaring).Invoke(this, new object[0]) as ITypeDescriptor;
        }

        private IPropertyDescriptor<TDeclaring> DescribeProperty<TDeclaring>(
            PropertyInfo prop,
            Lazy<ITypeDescriptor<TDeclaring>> declaringType)
        {
            var propertyType = DescribeLazy(this, prop.PropertyType);

            var getter = WrapGetter(typeof(TDeclaring), prop);
            var setter = WrapSetter(typeof(TDeclaring), prop);

            Func
                <PropertyInfo, Lazy<ITypeDescriptor<TDeclaring>>, Lazy<ITypeDescriptor<object>>, Expression<Func<TDeclaring, object>>,
                    Expression<Func<TDeclaring, object, TDeclaring>>, IPropertyDescriptor<TDeclaring, object>> foo =
                        this.DescribeProperty;

            var method = foo.Method.GetGenericMethodDefinition().MakeGenericMethod(typeof(TDeclaring), prop.PropertyType);

            return method.Invoke(this, new object[] { prop, declaringType, propertyType, getter, setter }) as IPropertyDescriptor<TDeclaring>;
        }

        private IPropertyDescriptor<TDeclaring, TProperty> DescribeProperty<TDeclaring, TProperty>(
            PropertyInfo prop,
            Lazy<ITypeDescriptor<TDeclaring>> declaringType,
            Lazy<ITypeDescriptor<TProperty>> propertyType,
            Expression<Func<TDeclaring, TProperty>> get,
            Expression<Func<TDeclaring, TProperty, TDeclaring>> set)
        {
            var getCompiled = get == null ? null : get.Compile();
            var setCompiled = set == null ? null : set.Compile();

            return new PropertyDescriptor<TDeclaring, TProperty>(prop, declaringType, propertyType, getCompiled, setCompiled, get, set);
        }
    }
    //// ReSharper restore AccessToModifiedClosure
}
