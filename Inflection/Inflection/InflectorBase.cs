namespace Inflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Immutable.TypeSystem;

    // TODO: support conventions in Immutable inflector for setting values via 1) copy-ctor, 2) ctor, 3) optional with-method, 4) non-optional with-method
    public abstract class InflectorBase : IInflector
    {
        protected readonly Dictionary<Type, IImmutableType> cachedDescriptors = new Dictionary<Type, IImmutableType>();

        public virtual IImmutableType<TDeclaring> Inflect<TDeclaring>()
        {
            IImmutableType cacheEntry;

            if (this.cachedDescriptors.TryGetValue(typeof(TDeclaring), out cacheEntry))
            {
                return (IImmutableType<TDeclaring>)cacheEntry;
            }

            ImmutableType<TDeclaring> desc = null;

            var tdLazy = new Lazy<IImmutableType<TDeclaring>>(() => desc);

            var props = typeof(TDeclaring).GetProperties()
                                          .Where(x => x.CanRead)
                                          .Where(x => !x.GetIndexParameters().Any())
                                          .Select(p => this.DescribeProperty(p, tdLazy));

            desc = new ImmutableType<TDeclaring>(this, props);

            this.cachedDescriptors[typeof(TDeclaring)] = desc;

            return desc;
        }

        public virtual IImmutableType Inflect(Type tDeclaring)
        {
            var method = this.GetType()
                             .GetMethods()
                             .Where(x => x.Name == "Inflect")
                             .Where(x => !x.GetParameters().Any())
                             .Where(x => x.IsGenericMethodDefinition)
                             .First(x => x.GetGenericArguments().Count() == 1);

            return method.MakeGenericMethod(tDeclaring).Invoke(this, new object[0]) as IImmutableType;
        }

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

        protected abstract IImmutablePropertyMember<TDeclaring> DescribeProperty<TDeclaring>(
            PropertyInfo prop,
            Lazy<IImmutableType<TDeclaring>> declaringType);
        
        protected static class DefaultHelper
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