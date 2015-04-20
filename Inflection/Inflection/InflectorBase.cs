namespace Inflection
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Immutable;
    using Immutable.Monads;
    using Immutable.TypeSystem;

    // TODO: support conventions in Immutable inflector for setting values via 1) copy-ctor, 2) ctor, 3) optional with-method, 4) non-optional with-method
    public abstract class InflectorBase : IInflector
    {
        public abstract IImmutableType<TDeclaring> Inflect<TDeclaring>();

        public abstract IImmutableType Inflect(Type tDeclaring);

        protected static object DescribeLazy(IInflector inflector, Type tDeclaring)
        {
            var descType = typeof(IImmutableType<>).MakeGenericType(tDeclaring);

            var funcType = typeof(Func<>).MakeGenericType(descType);
            var lazyType = typeof(Lazy<>).MakeGenericType(descType);

            var lazyCtor = lazyType.GetConstructor(new[] { funcType });
            if (lazyCtor == null)
            {
                throw new Exception("Can't find .ctor of Lazy<" + tDeclaring.Name + ">");
            }

            var method = typeof(IInflector).GetMethod("Inflect", Type.EmptyTypes)
                                           .MakeGenericMethod(tDeclaring);

            var func = Expression.Lambda(funcType, Expression.Call(Expression.Constant(inflector), method)).Compile();

            return lazyCtor.Invoke(new object[] { func });
        }

        protected static Expression WrapGetter(Type tDeclaring, PropertyInfo prop)
        {
            if (!prop.CanRead)
            {
                return null;
            }

            var tProp = prop.PropertyType;

            if (tDeclaring == null)
            {
                throw new ArgumentException("Cannot get declaring type for property " + prop.Name);
            }

            var funcType = typeof(Func<,>).MakeGenericType(tDeclaring, tProp);

            var p0 = Expression.Parameter(tDeclaring, tDeclaring.Name.ToLower().Substring(0, 1));
            var body = Expression.MakeMemberAccess(p0, prop);

            return Expression.Lambda(funcType, body, p0);
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

            var justCtor = just.GetConstructor(new[] {exprType});
            return (IMaybe<Expression>)justCtor.Invoke(new object[] { expr });
        }

        private static class DefaultHelper
        {
            public static object GetDefault(Type t)
            {
                return typeof(DefaultHelper).GetMethods()
                                            .First(x => x.IsGenericMethodDefinition)
                                            .MakeGenericMethod(t)
                                            .Invoke(null, null);
            }

            public static T GetDefault<T>()
            {
                return default(T);
            }
        }
    }
}