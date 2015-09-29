namespace Inflection.TypeGraph.Graph.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Immutable.Extensions;
    using Immutable.Monads;
    using Immutable.TypeSystem;

    using Strategies;

    using Visitors;

    public static class TypeDescendant
    {
        public static TypeDescendant<TRoot, TParent, TNode> Create<TRoot, TParent, TNode>(
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

            var getPath = parent.GetPath + "." + property.ClrMember.Name;

            var set = parent.Set.Bind(x => MergeSet(x, parent.Get, property.With));

            return new TypeDescendant<TRoot, TParent, TNode>(parent.IsMemoizing, rootType, property.PropertyType, get, getPath, set);
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
    }

    public class TypeDescendant<TRoot, TParent, TNode> : ITypeDescendant<TRoot, TNode>
    {
        private readonly IImmutableType<TNode> nodeType;
        private readonly IImmutableType<TRoot> rootType;

        private readonly Func<TRoot, TNode> get;
        private readonly string getPath;
        private readonly IMaybe<Func<TRoot, TNode, TRoot>> set;

        private readonly Lazy<ImmutableDictionary<MemberInfo, ITypeDescendant<TRoot>>> children;

        private readonly Dictionary<TRoot, TNode> memoizationCache = new Dictionary<TRoot, TNode>();

        public TypeDescendant(
            bool isMemoizing,
            IImmutableType<TRoot> rootType,
            IImmutableType<TNode> nodeType,
            Func<TRoot, TNode> get,
            string getPath,
            IMaybe<Func<TRoot, TNode, TRoot>> set)
        {
            this.IsMemoizing = isMemoizing;
            this.nodeType = nodeType;
            this.rootType = rootType;

            this.get = this.IsMemoizing ? this.Memoify(get) : get;
            this.getPath = getPath;
            this.set = this.IsMemoizing ? this.Memoify(set) : set;

            this.children = new Lazy<ImmutableDictionary<MemberInfo, ITypeDescendant<TRoot>>>(
                () => ImmutableDictionary.CreateRange(
                    MemberInfoEqualityComparer.Default,
                    this.CreateChildren(nodeType.GetProperties())));
        }

        public bool IsMemoizing { get; }

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

        public Func<TRoot, TNode> Get
        {
            get { return this.get; }
        }

        public string GetPath {
            get { return this.getPath; }
        }

        public IMaybe<Func<TRoot, TNode, TRoot>> Set
        {
            get { return this.set; }
        }

        protected ImmutableDictionary<MemberInfo, ITypeDescendant<TRoot>> Children
        {
            get { return this.children.Value; }
        }

        public IMaybe<ITypeDescendant<TRoot>> MaybeGetChild(MemberInfo memberInfo)
        {
            return this.Children.MaybeGetValue(memberInfo);
        }

        IEnumerable<ITypeDescendant> ITypeDescendant.GetChildren()
        {
            return this.Children.Values;
        }

        void ITypeDescendant.Accept(ITypeDescendantVisitor visitor)
        {
            visitor.Visit(this);
        }

        void ITypeDescendant<TRoot>.Accept(ITypeDescendantVisitor<TRoot> visitor)
        {
            visitor.Visit(this);
        }

        IEnumerable<ITypeDescendant<TRoot>> ITypeDescendant<TRoot>.GetChildren()
        {
            return this.Children.Values;
        }

        public TRoot Update(TRoot root, Func<TNode, TNode> f)
        {
            var v = this.Get(root);

            return this.Set.FMap(x => x(root, f(v))).GetValueOrDefault(root);
        }

        public IMaybe<ITypeDescendant<TRoot, T>> MaybeGetChild<T>(Expression<Func<TNode, T>> propertyExpr)
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

        public IMaybe<ITypeDescendant<TRoot, T>> MaybeGetDescendant<T>(Expression<Func<TNode, T>> propertyExpr)
        {
            var members = Unroll(propertyExpr).Reverse().GetEnumerator();

            return this.GetDescendant<T>(members);
        }

        public IEnumerable<ITypeDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>()
        {
            var strategy = DfsStrategy.Create<TRoot>()
                                      .WithCycleDetector();

            return GetDescendantsInternal<TDescendant>(this, strategy);
        }

        public IEnumerable<ITypeDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>(TRoot root)
        {
            var strategy = DfsStrategy.Create<TRoot>()
                                      .WithCycleDetector()
                                      .WithNullCheck(root);

            return GetDescendantsInternal<TDescendant>(this, strategy);
        }

        public IEnumerable<ITypeDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>(IDescendingStrategy<TRoot> descendingStrategy)
        {
            return GetDescendantsInternal<TDescendant>(this, descendingStrategy);
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
            IDescendingStrategy<TRoot> descendingStrategy)
        {
            descendingStrategy.Enter(typeDescendant);

            try
            {
                var children = descendingStrategy.GetChildren(typeDescendant);

                foreach (var c in children)
                {
                    if (c is ITypeDescendant<TRoot, TDescendant>)
                    {
                        yield return (ITypeDescendant<TRoot, TDescendant>)c;
                    }

                    foreach (var d in GetDescendantsInternal<TDescendant>(c, descendingStrategy))
                    {
                        yield return d;
                    }
                }
            } finally
            {
                descendingStrategy.Leave(typeDescendant);
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

        protected IMaybe<Func<TRoot, TNode, TRoot>> Memoify(IMaybe<Func<TRoot, TNode, TRoot>> mset) {
            return mset.FMap(
                            x => {
                                return new Func<TRoot, TNode, TRoot>((y, z) => {
                                    this.memoizationCache.Clear();

                                    return x(y, z);
                                });
                            });
        }

        protected IMaybe<ITypeDescendant<TRoot, T>> GetDescendant<T>(IEnumerator<MemberInfo> members)
        {
            var descendant = Maybe.Return<ITypeDescendant<TRoot>>(this);

            while (descendant is Just<ITypeDescendant<TRoot>> && members.MoveNext())
            {
                descendant = descendant.Bind(x => x.MaybeGetChild(members.Current));
            }

            return descendant.Bind(x => Maybe.Return(x as ITypeDescendant<TRoot, T>));
        }

        protected IEnumerable<KeyValuePair<MemberInfo, ITypeDescendant<TRoot>>> CreateChildren(IEnumerable<IImmutablePropertyMember<TNode>> props)
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