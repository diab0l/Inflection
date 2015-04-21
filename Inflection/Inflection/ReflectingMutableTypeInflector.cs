namespace Inflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using global::Inflection.Immutable;

    using Immutable.Monads;
    using Immutable.TypeSystem;

    //// ReSharper disable AccessToModifiedClosure
    public class ReflectingMutableTypeInflector : InflectorBase
    {
        private readonly Dictionary<Type, IImmutableType> cachedDescriptors = new Dictionary<Type, IImmutableType>();

        public override IImmutableType<TDeclaring> Inflect<TDeclaring>()
        {
            if (this.cachedDescriptors.ContainsKey(typeof(TDeclaring)))
            {
                return this.cachedDescriptors[typeof(TDeclaring)] as IImmutableType<TDeclaring>;
            }

            ImmutableType<TDeclaring> desc = null;

            var tdLazy = new Lazy<IImmutableType<TDeclaring>>(() => desc);

            var props = typeof(TDeclaring).GetProperties().Select(p => this.DescribeProperty(p, tdLazy));

            desc = new ImmutableType<TDeclaring>(props);

            this.cachedDescriptors[typeof(TDeclaring)] = desc;

            return desc;
        }

        public override IImmutableType Inflect(Type tDeclaring)
        {
            var method = typeof(ReflectingMutableTypeInflector).GetMethod("Describe", Type.EmptyTypes);

            return method.MakeGenericMethod(tDeclaring).Invoke(this, new object[0]) as IImmutableType;
        }

        private IImmutableProperty<TDeclaring> DescribeProperty<TDeclaring>(
            PropertyInfo prop,
            Lazy<IImmutableType<TDeclaring>> declaringType)
        {
            var propertyType = DescribeLazy(this, prop.PropertyType);

            var getter = WrapGetter(typeof(TDeclaring), prop);
            var setter = WrapSetter(typeof(TDeclaring), prop);

            Func<PropertyInfo, 
                 Lazy<IImmutableType<TDeclaring>>, 
                 Lazy<IImmutableType<object>>, 
                 Expression<Func<TDeclaring, object>>,
                 IMaybe<Expression<Func<TDeclaring, object, TDeclaring>>>, 
                 IImmutableProperty<TDeclaring, object>> foo =
                        this.DescribeProperty;

            var method = foo.Method.GetGenericMethodDefinition().MakeGenericMethod(typeof(TDeclaring), prop.PropertyType);

            return method.Invoke(this, new object[] { prop, declaringType, propertyType, getter, setter }) as IImmutableProperty<TDeclaring>;
        }

        private IImmutableProperty<TDeclaring, TProperty> DescribeProperty<TDeclaring, TProperty>(
            PropertyInfo prop,
            Lazy<IImmutableType<TDeclaring>> declaringType,
            Lazy<IImmutableType<TProperty>> propertyType,
            Expression<Func<TDeclaring, TProperty>> get,
            IMaybe<Expression<Func<TDeclaring, TProperty, TDeclaring>>> set)
        {
            if (get == null)
            {
                throw new ArgumentNullException("get");
            }

            var getCompiled = get.Compile();
            var setCompiled = set.Bind(x => Maybe.Return(x.Compile()));

            return new ImmutableProperty<TDeclaring, TProperty>(prop, declaringType, propertyType, getCompiled, setCompiled, get, set);
        }
    }
    //// ReSharper restore AccessToModifiedClosure
}
