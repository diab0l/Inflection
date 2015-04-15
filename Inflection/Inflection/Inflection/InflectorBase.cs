namespace Inflection.Inflection
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using Immutable;

    // TODO: support conventions in Immutable describer
    // TODO: implement conventions for setting values via 1) copy-ctor, 2) ctor, 3) optional with-method, 4) non-optional with-method
    public abstract class InflectorBase : IInflector
    {
        public abstract ITypeDescriptor<TDeclaring> Inflect<TDeclaring>();

        public abstract ITypeDescriptor Inflect(Type tDeclaring);

        protected static object DescribeLazy(IInflector inflector, Type tDeclaring)
        {
            var descType = typeof(ITypeDescriptor<>).MakeGenericType(tDeclaring);

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

        protected static Expression WrapSetter(Type tDeclaring, PropertyInfo prop)
        {
            if (!prop.CanWrite)
            {
                return null;
            }

            var tProp = prop.PropertyType;

            if (tDeclaring == null)
            {
                throw new ArgumentException("Cannot get declaring type for property " + prop.Name);
            }

            var funcType = typeof(Func<,,>).MakeGenericType(tDeclaring, tProp, tDeclaring);

            var p0 = Expression.Parameter(tDeclaring, tDeclaring.Name.ToLower().Substring(0, 1));
            var p1 = Expression.Parameter(tProp, "value");

            // (s, v) => { s.Prop = v; return s; }
            var assign = Expression.Assign(Expression.MakeMemberAccess(p0, prop), p1);
            var body = Expression.Block(
                assign,
                Expression.Label(Expression.Label(tDeclaring), p0));

            return Expression.Lambda(funcType, body, p0, p1);
        }
    }
}