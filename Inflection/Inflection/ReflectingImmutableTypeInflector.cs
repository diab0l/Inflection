namespace Inflection
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Immutable.Monads;
    using Immutable.TypeSystem;

    public class ReflectingImmutableTypeInflector : InflectorBase
    {
        protected override IImmutablePropertyMember<TDeclaring> DescribeProperty<TDeclaring>(
            PropertyInfo prop,
            Lazy<IImmutableType<TDeclaring>> declaringType)
        {
            var propertyType = DescribeLazy(this, prop.PropertyType);

            var getter = WrapGetter(typeof(TDeclaring), prop);
            var setter = WrapCtor(typeof(TDeclaring), prop);

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

        private static IMaybe<Expression> WrapWither(Type tDeclaring, PropertyInfo prop)
        {
            if (tDeclaring == null)
            {
                throw new ArgumentNullException(nameof(tDeclaring));
            }

            var tProp = prop.PropertyType;

            var funcType = typeof(Func<,,>).MakeGenericType(tDeclaring, tProp, tDeclaring);
            var exprType = typeof(Expression<>).MakeGenericType(funcType);

            var methodName = "With" + prop.Name;

            var wither = tDeclaring.GetMethod(methodName, new[] { prop.PropertyType });

            if (wither == null || wither.ReturnType != tDeclaring)
            {
                var nothing = typeof(Nothing<>).MakeGenericType(exprType);

                return (IMaybe<Expression>)DefaultHelper.GetDefault(nothing);
            }

            var p0 = Expression.Parameter(tDeclaring, tDeclaring.Name.ToLower().Substring(0, 1));
            var p1 = Expression.Parameter(tProp, "value");

            // (s, v) => s.WithFoo(v)
            var body = Expression.Call(p0, wither, p1);

            var expr = Expression.Lambda(funcType, body, p0, p1);

            var just = typeof(Just<>).MakeGenericType(exprType);

            var justCtor = just.GetConstructor(new[] { exprType });
            return (IMaybe<Expression>)justCtor.Invoke(new object[] { expr });
        }

        private static IMaybe<Expression> WrapCtor(Type tDeclaring, PropertyInfo prop)
        {
            if (tDeclaring == null)
            {
                throw new ArgumentNullException(nameof(tDeclaring));
            }

            var tProp = prop.PropertyType;

            var funcType = typeof(Func<,,>).MakeGenericType(tDeclaring, tProp, tDeclaring);
            var exprType = typeof(Expression<>).MakeGenericType(funcType);

            var copyCtor = (ConstructorInfo)null;
            var propParam = (ParameterInfo)null;

            foreach (var ctor in tDeclaring.GetConstructors())
            {
                var ps = ctor.GetParameters();

                if (ps.Length == 0)
                {
                    continue;
                }

                if (ps[0].ParameterType != tDeclaring)
                {
                    continue;
                }

                var allOptional = true;

                foreach (var p in ps.Skip(1))
                {
                    if (!p.IsOptional)
                    {
                        allOptional = false;
                        break;
                    }

                    if (!string.Equals(p.Name, prop.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!p.ParameterType.IsAssignableFrom(tProp))
                    {
                        continue;
                    }

                    propParam = p;
                }

                if (!allOptional || propParam == null)
                {
                    continue;
                }

                copyCtor = ctor;
                break;
            }

            if (copyCtor == null)
            {
                var nothing = typeof(Nothing<>).MakeGenericType(exprType);

                return (IMaybe<Expression>)DefaultHelper.GetDefault(nothing);
            }

            var p0 = Expression.Parameter(tDeclaring, tDeclaring.Name.ToLower().Substring(0, 1));
            var p1 = Expression.Parameter(tProp, "value");

            var pars = copyCtor.GetParameters();
            var pExprs = new Expression[pars.Length];

            pExprs[0] = p0;

            for (var i = 1; i < pars.Length; i++)
            {
                if (pars[i] == propParam)
                {
                    pExprs[i] = p1;

                    var propParamType = propParam.ParameterType;
                    if (propParamType.IsGenericType && propParamType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var ctor = propParamType.GetConstructors()[0];

                        pExprs[i] = Expression.New(ctor, p1);
                    }
                }
                else
                {
                    pExprs[i] = Expression.Constant(pars[i].DefaultValue, pars[i].ParameterType);
                }
            }

            // (s, v) => new T(s, Prop: v)
            var body = Expression.New(copyCtor, pExprs);

            var expr = Expression.Lambda(funcType, body, p0, p1);

            var just = typeof(Just<>).MakeGenericType(exprType);

            var justCtor = just.GetConstructor(new[] { exprType });
            return (IMaybe<Expression>)justCtor.Invoke(new object[] { expr });
        }
    }
}