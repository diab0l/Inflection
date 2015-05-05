namespace Inflection.Immutable.Graph.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net.Configuration;
    using System.Reflection;

    using Extensions;

    using Monads;

    using Strategies;

    using TypeSystem;

    using Visitors;

    public static class TypeDescendant
    {
        public static TypeDescendant<TRoot, TNode> Create<TRoot, TParent, TNode>(
            ITypeDescendant<TRoot, TParent> parent,
            IImmutableProperty<TParent, TNode> property)
        {
            var rootType = parent.RootType;
            var get = typeof(TParent).IsValueType ?
                new Func<TRoot, TNode>(x => property.Get(parent.Get(x))) :
                (x =>
                {
                    var y = parent.Get(x);

                    return y == null ? default(TNode) : property.Get(y);
                });
            var set = parent.Set.Bind(x => MergeSet(x, parent.Get, property.Set));

            var lazyGetExpr = new Lazy<Expression<Func<TRoot, TNode>>>(() => MergeGetExpressions(parent.GetExpression, property.GetExpression));
            var lazySetExpr = new Lazy<IMaybe<Expression<Func<TRoot, TNode, TRoot>>>>(() => MergeSetExpressions(parent.GetExpression, parent.SetExpression, property.SetExpression));

            return new TypeDescendant<TRoot, TNode>(parent.IsMemoizing, rootType, property.PropertyType, get, set, lazyGetExpr, lazySetExpr);
        }

        private static IMaybe<Func<TRoot, TNode, TRoot>> MergeSet<TRoot, TParent, TNode>(Func<TRoot, TParent, TRoot> parentSet, Func<TRoot, TParent> parentGet, IMaybe<Func<TParent, TNode, TParent>> maybePropertySet)
        {
            var propertySet = maybePropertySet.GetValueOrDefault();
            if (propertySet == null)
            {
                return new Nothing<Func<TRoot, TNode, TRoot>>();
            }

            Func<TRoot, TNode, TRoot> set = (x, y) => parentSet(x, propertySet(parentGet(x), y));
            return Maybe.Return(set);
        }

        private static Expression<Func<TRoot, TNode>> MergeGetExpressions<TRoot, TParent, TNode>(
            Expression<Func<TRoot, TParent>> parentGet,
            Expression<Func<TParent, TNode>> nodeGet)
        {
            if (parentGet == null || nodeGet == null)
            {
                return null;
            }

            //// parentGet: x => ((x).Foo).Bar
            //// nodeGet  : y => ((y).Baz).Ban

            //// get      : x => ((((x).Foo).Bar).Baz).Ban
            //// 
            //// Replace every occurence of param y in nodeGet with parentGet expr
            var x = parentGet.Parameters[0];
            var y = nodeGet.Parameters[0];

            var body = nodeGet.Body.Replace(y, parentGet.Body);

            var get = Expression.Lambda<Func<TRoot, TNode>>(body, x);

            return get;
        }

        private static IMaybe<Expression<Func<TRoot, TNode, TRoot>>> MergeSetExpressions<TRoot, TParent, TNode>(
            Expression<Func<TRoot, TParent>> parentGet,
            IMaybe<Expression<Func<TRoot, TParent, TRoot>>> maybeParentSet,
            IMaybe<Expression<Func<TParent, TNode, TParent>>> maybeNodeSet)
        {
            var parentSet = maybeParentSet.GetValueOrDefault();
            var nodeSet = maybeNodeSet.GetValueOrDefault();

            if (parentSet == null || nodeSet == null)
            {
                return new Nothing<Expression<Func<TRoot, TNode, TRoot>>>();
            }

            //// Func<TRoot, TNode, TRoot> set = (x, z) => parentSet(x, nodeSet(parentGet(x), z));

            //// parentSet: (r, y) => r
            //// parentSet: (r, y) => setX(r, setY(getY(x), y))
            //// nodeSet:   (p, z) => setZ(p, z)

            //// get:       (r, z) => setX(r, setY(getX(r), setZ(getY(r), z)))

            var r = parentSet.Parameters[0];
            var y = parentSet.Parameters[1];
            var p = nodeSet.Parameters[0];
            var z = nodeSet.Parameters[1];

            var g = parentGet.Parameters[0];

            var getParent = parentGet.Body.Replace(g, r);
            var rootedNodeSet = nodeSet.Body.Replace(p, getParent);
            var body = parentSet.Body.Replace(y, rootedNodeSet);

            var set = Expression.Lambda<Func<TRoot, TNode, TRoot>>(body, r, z);

            return Maybe.Return(set);
        }
    }

    public class TypeDescendant<TRoot, TNode> : ITypeDescendant<TRoot, TNode>
    {
        private readonly bool isMemoizing;
        
        private readonly IImmutableType<TNode> nodeType;
        private readonly IImmutableType<TRoot> rootType;

        private readonly Func<TRoot, TNode> get;
        private readonly IMaybe<Func<TRoot, TNode, TRoot>> set;

        private readonly Lazy<Expression<Func<TRoot, TNode>>> getExpression;
        private readonly Lazy<IMaybe<Expression<Func<TRoot, TNode, TRoot>>>> setExpression;

        private readonly Lazy<ImmutableDictionary<MemberInfo, ITypeDescendant<TRoot>>> children;

        private readonly Dictionary<TRoot, TNode> memoizationCache = new Dictionary<TRoot, TNode>();
        
        public TypeDescendant(
            bool isMemoizing,
            IImmutableType<TRoot> rootType,
            IImmutableType<TNode> nodeType,
            Func<TRoot, TNode> get,
            IMaybe<Func<TRoot, TNode, TRoot>> set,
            Lazy<Expression<Func<TRoot, TNode>>> getExpression,
            Lazy<IMaybe<Expression<Func<TRoot, TNode, TRoot>>>> setExpression)
        {
            this.isMemoizing = isMemoizing;
            this.nodeType = nodeType;
            this.rootType = rootType;

            this.get = this.isMemoizing ? this.Memoify(get) : get;
            
            this.set = set;
            this.getExpression = getExpression;
            this.setExpression = setExpression;

            this.children = new Lazy<ImmutableDictionary<MemberInfo, ITypeDescendant<TRoot>>>(() => ImmutableDictionary.CreateRange(this.CreateChildren(nodeType.GetProperties())));
        }

        public bool IsMemoizing
        {
            get { return this.isMemoizing; }
        }

        IImmutableType ITypeDescendant.RootType
        {
            get { return this.rootType; }
        }

        public IImmutableType<TRoot> RootType
        {
            get { return this.rootType; }
        }

        IImmutableType ITypeDescendant.NodeType
        {
            get { return this.nodeType; }
        }

        public IImmutableType<TNode> NodeType
        {
            get { return this.nodeType; }
        }

        Expression ITypeDescendant.GetExpression
        {
            get { return this.getExpression.Value; }
        }

        public Expression<Func<TRoot, TNode>> GetExpression
        {
            get { return this.getExpression.Value; }
        }

        IMaybe<Expression> ITypeDescendant.SetExpression
        {
            get { return this.setExpression.Value; }
        }

        public IMaybe<Expression<Func<TRoot, TNode, TRoot>>> SetExpression
        {
            get { return this.setExpression.Value; }
        }

        public Func<TRoot, TNode> Get
        {
            get { return this.get; }
        }

        public IMaybe<Func<TRoot, TNode, TRoot>> Set
        {
            get { return this.set; }
        }

        protected ImmutableDictionary<MemberInfo, ITypeDescendant<TRoot>> Children
        {
            get { return this.children.Value; }
        }

        IEnumerable<ITypeDescendant> ITypeDescendant.GetChildren()
        {
            return this.Children.Values;
        }

        void ITypeDescendant.Accept(ITypeDescendantVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(ITypeDescendantChildrenVisitor visitor)
        {
            visitor.Visit<TRoot, TNode>(this.Children.MaybeGetValue);
        }

        void ITypeDescendant<TRoot>.Accept(ITypeDescendantVisitor<TRoot> visitor)
        {
            visitor.Visit(this);
        }

        void ITypeDescendant<TRoot>.Accept(ITypeDescendantChildrenVisitor<TRoot> visitor)
        {
            visitor.Visit<TNode>(this.Children.MaybeGetValue);
        }

        IEnumerable<ITypeDescendant<TRoot>> ITypeDescendant<TRoot>.GetChildren()
        {
            return this.Children.Values;
        }

        public IMaybe<ITypeDescendant<TRoot, T>> GetChild<T>(Expression<Func<TNode, T>> propertyExpr)
        {
            var memExpr = propertyExpr.Body as MemberExpression;

            if (memExpr == null)
            {
                return new Nothing<ITypeDescendant<TRoot, T>>();
            }

            return this.Children.MaybeGetValue(memExpr.Member).FMap(x => x as ITypeDescendant<TRoot, T>);
        }

        public IEnumerable<ITypeDescendant<TRoot, T>> GetChildren<T>()
        {
            return this.Children.Values.OfType<ITypeDescendant<TRoot, T>>();
        }

        public IMaybe<ITypeDescendant<TRoot, T>> GetDescendant<T>(Expression<Func<TNode, T>> propertyExpr)
        {
            var members = Unroll(propertyExpr).Reverse().GetEnumerator();

            return this.GetDescendant<T>(members);
        }

        public IMaybe<IObjectDescendant<TRoot, TNode>> Close(TRoot value)
        {
            var og = ObjectGraph.Create(null, value);

            return og.GetDescendant(this.GetExpression);
        }

        public IEnumerable<ITypeDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>()
        {
            var seenTypes = new Dictionary<IImmutableType, int>();
            Func<ITypeDescendant<TRoot>, IEnumerable<ITypeDescendant<TRoot>>> getChildren = x =>
            {
                if (seenTypes.GetValueOrDefault(x.NodeType) > 1)
                {
                    throw new Exception("Cyclic graph detected");
                }

                return x.GetChildren();
            };

            Action<ITypeDescendant<TRoot>> enter = x => seenTypes.AddOrUpdate(x.NodeType, 1, y => y + 1);
            Action<ITypeDescendant<TRoot>> leave = x => seenTypes.AddOrUpdate(x.NodeType, 0, y => y - 1);

            return GetDescendantsInternal<TDescendant>(this, new DescendingStrategy<TRoot>(getChildren), enter, leave);
        }

        public IEnumerable<ITypeDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>(IDescendingStrategy<TRoot> descendingStrategy)
        {
            return GetDescendantsInternal<TDescendant>(this, descendingStrategy, null, null);
        }

        public override string ToString()
        {
            return this.GetExpression.ToString();
        }

        protected static IEnumerable<MemberInfo> Unroll<T1, T2>(Expression<Func<T1, T2>> expr)
        {
            var body = expr.Body as MemberExpression;

            while (body != null)
            {
                yield return body.Member;

                body = body.Expression as MemberExpression;
            }
        }

        protected static IEnumerable<ITypeDescendant<TRoot, TDescendant>> GetDescendantsInternal<TDescendant>(
            ITypeDescendant<TRoot> typeDescendant,
            IDescendingStrategy<TRoot> descendingStrategy,
            Action<ITypeDescendant<TRoot>> enter,
            Action<ITypeDescendant<TRoot>> leave)
        {
            if (enter != null)
            {
                enter(typeDescendant);
            }

            try
            {
                foreach (var c in descendingStrategy.GetChildren(typeDescendant))
                {
                    if (c is ITypeDescendant<TRoot, TDescendant>)
                    {
                        yield return (ITypeDescendant<TRoot, TDescendant>)c;
                    }

                    foreach (var d in GetDescendantsInternal<TDescendant>(c, descendingStrategy, enter, leave))
                    {
                        yield return d;
                    }
                }
            } finally
            {
                if (leave != null)
                {
                    leave(typeDescendant);
                }
            }
        }

        protected Func<TRoot, TNode> Memoify(Func<TRoot, TNode> get)
        {
            return root =>
            {
                TNode v;

                if (!this.memoizationCache.TryGetValue(root, out v))
                {
                    v = get(root);
                    this.memoizationCache[root] = v;
                }

                if (this.memoizationCache.Count > 10)
                {
                    this.memoizationCache.Clear();
                }

                return v;
            };            
        } 

        protected IMaybe<ITypeDescendant<TRoot, T>> GetDescendant<T>(IEnumerator<MemberInfo> members)
        {
            var descendant = Maybe.Return<ITypeDescendant<TRoot>>(this);

            var visitor = new ChildFinderVisitor<TRoot>();
            while (!descendant.IsEmpty && members.MoveNext())
            {
                descendant = descendant.Bind(x => visitor.MaybeGetChild(x, members.Current));
            }

            return descendant.Bind(x => Maybe.Return(x as ITypeDescendant<TRoot, T>));
        }

        protected IEnumerable<KeyValuePair<MemberInfo, ITypeDescendant<TRoot>>> CreateChildren(IEnumerable<IImmutableProperty<TNode>> props)
        {
            var builder = new TypeDescendantBuilderVisitor<TRoot, TNode>();

            foreach (var p in props)
            {
                var key = p.ClrMember;
                var value = builder.Build(this, p);

                yield return new KeyValuePair<MemberInfo, ITypeDescendant<TRoot>>(key, value);
            }
        }
    }
}