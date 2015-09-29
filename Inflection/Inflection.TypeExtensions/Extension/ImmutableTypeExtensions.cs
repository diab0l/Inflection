namespace Inflection.TypeExtensions.Extension
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Immutable.TypeSystem;

    // TODO: add correctness tests for inheritance stuff
    public static class ImmutableTypeExtensions
    {
        private static InflectorCache inflectorCache;

        public static IEnumerable<T> GAsEnumerable<TType, T>(this TType root, IInflector inflector)
        {
            return GetValues<TType, T>(root, inflector).Select(x => x.Item1);
        }

        public static IEnumerable<TB> GSelect<TType, TA, TB>(this TType root, Func<TA, TB> f, IInflector inflector)
        {
            return root.GAsEnumerable<TType, TA>(inflector).Select(f);
        }

        public static TRoot GMap<TRoot, TA, TB>(this TRoot root, Func<TA, TB> f, IInflector inflector)
            where TB : TA
        {
            if (inflectorCache == null || inflectorCache.Inflector != inflector)
            {
                inflectorCache = new InflectorCache(inflector);
            }

            IImmutableType tRoot;
            if (!inflectorCache.Types.TryGetValue(typeof(TRoot), out tRoot))
            {
                tRoot = inflector.Inflect<TRoot>();
                inflectorCache.Types[typeof(TRoot)] = tRoot;
            }

            IImmutableType tA;
            if (!inflectorCache.Types.TryGetValue(typeof(TA), out tA))
            {
                tA = inflector.Inflect<TA>();
                inflectorCache.Types[typeof(TA)] = tA;
            }
            
            IImmutableType tB;
            if (!inflectorCache.Types.TryGetValue(typeof(TB), out tB))
            {
                tB = inflector.Inflect<TB>();
                inflectorCache.Types[typeof(TB)] = tB;
            }
            
            return (TRoot)GMap(root, tRoot, f, tA, tB);
        }

        private static object GMap<TA, TB>(object nodeValue, IImmutableType tNode, Func<TA, TB> f, IImmutableType tA, IImmutableType tB)
        {
            var seed = nodeValue;

            if (nodeValue is TA && tB.Extends(tNode))
            {
                seed = f((TA)nodeValue);
            }

            var agg = seed;

            List<IImmutableProperty> props;
            if (!inflectorCache.Properties.TryGetValue(tNode, out props))
            {
                props = tNode.GetProperties().ToList();
                inflectorCache.Properties[tNode] = props;
            }

            foreach (var prop in props.Where(p => p.HasWither))
            {
                if (agg == null)
                {
                    break;
                }

                var pv = prop.GetValue(agg);
                var gv = GMap(pv, prop.PropertyType, f, tA, tB);
                agg = prop.WithValue(agg, gv);
            }

            return agg;
        }

        public static IEnumerable<Tuple<T, string>> GetValues<TType, T>(this TType root, IInflector inflector)
        {
            if (inflectorCache == null || inflectorCache.Inflector != inflector)
            {
                inflectorCache = new InflectorCache(inflector);
            }

            IImmutableType type;
            if (!inflectorCache.Types.TryGetValue(typeof(TType), out type))
            {
                type = inflector.Inflect<TType>();
                inflectorCache.Types[typeof(TType)] = type;
            }
            
            var stack = new Stack<Tuple<object, IImmutableType, string>>();
            stack.Push(new Tuple<object, IImmutableType, string>(root, type, "x => x"));

            while (stack.Count > 0)
            {
                var p = stack.Pop();
                var t = p.Item2;

                List<IImmutableProperty> props;

                if (!inflectorCache.Properties.TryGetValue(t, out props))
                {
                    props = t.GetProperties().ToList();
                    inflectorCache.Properties[t] = props;
                }

                foreach (var prop in props)
                {
                    var v = prop.GetValue(p.Item1);

                    if (v == null)
                    {
                        continue;
                    }

                    var tpl = new Tuple<object, IImmutableType, string>(v, prop.PropertyType, p.Item3 + "." + prop.ClrMember.Name);

                    if (v is T)
                    {
                        yield return Tuple.Create((T)v, tpl.Item3);
                    }

                    stack.Push(tpl);
                }
            }
        }

        private static IEnumerable<MemberInfo> Unroll<T1, T2>(Expression<Func<T1, T2>> expr)
        {
            var body = expr.Body as MemberExpression;

            while (body != null)
            {
                yield return body.Member;

                body = body.Expression as MemberExpression;
            }
        }

        private class InflectorCache
        {
            public readonly IInflector Inflector;

            public readonly Dictionary<Type, IImmutableType> Types = new Dictionary<Type, IImmutableType>();

            public readonly Dictionary<IImmutableType, List<IImmutableProperty>> Properties = new Dictionary<IImmutableType, List<IImmutableProperty>>();

            public InflectorCache(IInflector inflector)
            {
                this.Inflector = inflector;
            }
        }
    }
}