namespace Inflection
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using Immutable.Monads;
    using Immutable.TypeSystem;

    //// ReSharper disable AccessToModifiedClosure
    public class ReflectingMutableTypeInflector : InflectorBase
    {
        protected override IImmutablePropertyMember<TDeclaring> DescribeProperty<TDeclaring>(
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

            return method.Invoke(this, new object[] { prop, declaringType, propertyType, getter, setter }) as IImmutablePropertyMember<TDeclaring>;
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
                throw new ArgumentNullException(nameof(get));
            }

            var getCompiled = get.Compile();
            var setCompiled = set.Bind(x => Maybe.Return(x.Compile()));

            return new ImmutableProperty<TDeclaring, TProperty>(prop, declaringType, propertyType, getCompiled, setCompiled, get, set);
        }

        protected static IMaybe<Expression> WrapSetter(Type tDeclaring, PropertyInfo prop)
        {
            var tProp = prop.PropertyType;
            if (tDeclaring == null)
            {
                throw new ArgumentException("Cannot get declaring type for property " + prop.Name);
            }

            var funcType = typeof(Func<,,>).MakeGenericType(tDeclaring, tProp, tDeclaring);
            var exprType = typeof(Expression<>).MakeGenericType(funcType);

            if (!prop.CanWrite)
            {
                var nothing = typeof(Nothing<>).MakeGenericType(exprType);

                return (IMaybe<Expression>)DefaultHelper.GetDefault(nothing);
            }

            var p0 = Expression.Parameter(tDeclaring, tDeclaring.Name.ToLower().Substring(0, 1));
            var p1 = Expression.Parameter(tProp, "value");

            // (s, v) => { s.Prop = v; return s; }
            var assign = Expression.Assign(Expression.MakeMemberAccess(p0, prop), p1);
            var body = Expression.Block(
                                        assign,
                                        Expression.Label(Expression.Label(tDeclaring), p0));

            var expr = Expression.Lambda(funcType, body, p0, p1);

            var just = typeof(Just<>).MakeGenericType(exprType);

            var justCtor = just.GetConstructor(new[] { exprType });
            return (IMaybe<Expression>)justCtor.Invoke(new object[] { expr });
        }
    }
    //// ReSharper restore AccessToModifiedClosure
}
