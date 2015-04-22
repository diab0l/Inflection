namespace Inflection.Immutable.Graph.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Extensions;

    using Monads;

    using TypeSystem;

    using Visitors;

    public static class TypeDescendant
    {
        public static TypeDescendant<TRoot, TNode> Create<TRoot, TParent, TNode>(
            ITypeDescendant<TRoot, TParent> parent,
            IImmutableProperty<TParent, TNode> property)
        {
            var rootType = parent.RootType;
            Func<TRoot, TNode> get = x => property.Get(parent.Get(x));

            var set = parent.Set
                            .Bind(x => MergeSet(x, parent.Get, property.Set));
            
            var getExpr = MergeGetExpressions(parent.GetExpression, property.GetExpression);
            var setExpr = MergeSetExpressions(parent.GetExpression, parent.SetExpression, property.SetExpression);

            return new TypeDescendant<TRoot, TNode>(rootType, property.PropertyType, get, set, getExpr, setExpr);
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
        private readonly IImmutableType<TNode> nodeType;
        private readonly IImmutableType<TRoot> rootType;

        private readonly Func<TRoot, TNode> get;
        private readonly IMaybe<Func<TRoot, TNode, TRoot>> set;

        private readonly Expression<Func<TRoot, TNode>> getExpression;
        private readonly IMaybe<Expression<Func<TRoot, TNode, TRoot>>> setExpression;

        private readonly Lazy<ImmutableDictionary<MemberInfo, ITypeDescendant<TRoot>>> children;

        public TypeDescendant(
            IImmutableType<TRoot> rootType,
            IImmutableType<TNode> nodeType,
            Func<TRoot, TNode> get,
            IMaybe<Func<TRoot, TNode, TRoot>> set,
            Expression<Func<TRoot, TNode>> getExpression,
            IMaybe<Expression<Func<TRoot, TNode, TRoot>>> setExpression)
        {
            this.nodeType = nodeType;
            this.rootType = rootType;
            this.get = get;
            this.set = set;
            this.getExpression = getExpression;
            this.setExpression = setExpression;

            this.children = new Lazy<ImmutableDictionary<MemberInfo, ITypeDescendant<TRoot>>>(() => ImmutableDictionary.CreateRange(this.CreateChildren(nodeType.GetProperties())));
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
            get { return this.getExpression; }
        }

        public Expression<Func<TRoot, TNode>> GetExpression
        {
            get { return this.getExpression; }
        }

        IMaybe<Expression> ITypeDescendant.SetExpression
        {
            get { return this.setExpression; }
        }

        public IMaybe<Expression<Func<TRoot, TNode, TRoot>>> SetExpression
        {
            get { return this.setExpression; }
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
            visitor.Visit<TRoot, TNode>(this.Children.MaybeGetValue, this.Children.Values);
        }

        void ITypeDescendant<TRoot>.Accept(ITypeDescendantVisitor<TRoot> visitor)
        {
            visitor.Visit(this);
        }

        void ITypeDescendant<TRoot>.Accept(ITypeDescendantChildrenVisitor<TRoot> visitor)
        {
            visitor.Visit<TNode>(this.Children.MaybeGetValue, this.Children.Values);
        }

        IEnumerable<ITypeDescendant<TRoot>> ITypeDescendant<TRoot>.GetChildren()
        {
            return this.Children.Values;
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
            return GetDescendantsInternal<TDescendant>(this, ImmutableHashSet.Create<IImmutableType>());
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

        protected static IEnumerable<ITypeDescendant<TRoot, TDescendant>> GetDescendantsInternal<TDescendant>(ITypeDescendant<TRoot> typeDescendant, IImmutableSet<IImmutableType> seenTypes)
        {
            if (seenTypes.Contains(typeDescendant.NodeType))
            {
                throw new Exception("Cyclic graph detected");
            }

            var foo = new ChildExtractionVisitor<TRoot>();

            foreach (var c in foo.GetChildren(typeDescendant))
            {
                if (c is ITypeDescendant<TRoot, TDescendant>)
                {
                    yield return (ITypeDescendant<TRoot, TDescendant>)c;
                }

                foreach (var d in GetDescendantsInternal<TDescendant>(c, seenTypes.Add(typeDescendant.NodeType)))
                {
                    yield return d;
                }
            }
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