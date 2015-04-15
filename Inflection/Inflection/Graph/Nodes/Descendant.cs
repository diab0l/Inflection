namespace Inflection.Graph.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Immutable;

    using Visitors;

    public static class Descendant
    {
        public static Descendant<TRoot, TNode> Create<TRoot, TParent, TNode>(
            IDescendant<TRoot, TParent> parent,
            IPropertyDescriptor<TParent, TNode> property)
        {
            var rootType = parent.RootType;
            Func<TRoot, TNode> get = x => property.Get(parent.Get(x));
            Func<TRoot, TNode, TRoot> set = (x, y) => parent.Set(x, property.Set(parent.Get(x), y));
            
            var getExpr = MergeGetExpressions(parent.GetExpression, property.GetExpression);
            var setExpr = MergeSetExpressions(parent.GetExpression, parent.SetExpression, property.SetExpression);

            return new Descendant<TRoot, TNode>(rootType, property.PropertyType, get, set, getExpr, setExpr, property.PropertyType.GetProperties());
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

            var body = Replace(nodeGet.Body, y, parentGet.Body);
            
            var get = Expression.Lambda<Func<TRoot, TNode>>(body, x);

            return get;
        }

        private static Expression<Func<TRoot, TNode, TRoot>> MergeSetExpressions<TRoot, TParent, TNode>(
            Expression<Func<TRoot, TParent>> parentGet,
            Expression<Func<TRoot, TParent, TRoot>> parentSet,
            Expression<Func<TParent, TNode, TParent>> nodeSet)
        {
            if (parentSet == null || nodeSet == null)
            {
                return null;
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
            
            var getParent = Replace(parentGet.Body, g, r);
            var rootedNodeSet = Replace(nodeSet.Body, p, getParent);
            var body = Replace(parentSet.Body, y, rootedNodeSet);

            var set = Expression.Lambda<Func<TRoot, TNode, TRoot>>(body, r, z);

            return set;
        }

        private static Expression Replace(Expression @in, Expression old, Expression @new)
        {
            if (@in == old)
            {
                return @new;
            }

            if (@in is MemberExpression)
            {
                var mExpr = @in as MemberExpression;

                var expr = Replace(mExpr.Expression, old, @new);
                var member = mExpr.Member;

                return Expression.MakeMemberAccess(expr, member);
            }
           
            if (@in is BlockExpression)
            {
                var bExpr = @in as BlockExpression;

                var exprs = bExpr.Expressions.Select(x => Replace(x, old, @new));

                return Expression.Block(exprs);
            }

            if (@in is UnaryExpression)
            {
                var uExpr = @in as UnaryExpression;

                var op = Replace(uExpr.Operand, old, @new);

                return Expression.MakeUnary(uExpr.NodeType, op, uExpr.Type);
            }

            if (@in is BinaryExpression)
            {
                var bExpr = @in as BinaryExpression;
                var left = Replace(bExpr.Left, old, @new);
                var right = Replace(bExpr.Right, old, @new);

                return Expression.MakeBinary(bExpr.NodeType, left, right, bExpr.IsLiftedToNull, bExpr.Method, bExpr.Conversion);
            }

            if (@in is LabelExpression)
            {
                var lExpr = @in as LabelExpression;
                var dValue = Replace(lExpr.DefaultValue, old, @new);

                return Expression.Label(lExpr.Target, dValue);
            }

            return @in;
        }
    }

    public class Descendant<TRoot, TNode> : IDescendant<TRoot, TNode>
    {
        private readonly ITypeDescriptor<TNode> nodeType;
        private readonly ITypeDescriptor<TRoot> rootType;

        private readonly Func<TRoot, TNode> get;
        private readonly Func<TRoot, TNode, TRoot> set;

        private readonly Expression<Func<TRoot, TNode>> getExpression;
        private readonly Expression<Func<TRoot, TNode, TRoot>> setExpression;

        private readonly Lazy<ImmutableDictionary<MemberInfo, IDescendant<TRoot>>> children;

        public Descendant(
            ITypeDescriptor<TRoot> rootType,
            ITypeDescriptor<TNode> nodeType,
            Func<TRoot, TNode> get,
            Func<TRoot, TNode, TRoot> set,
            Expression<Func<TRoot, TNode>> getExpression,
            Expression<Func<TRoot, TNode, TRoot>> setExpression,
            IEnumerable<IPropertyDescriptor<TNode>> children)
        {
            this.nodeType = nodeType;
            this.rootType = rootType;
            this.get = get;
            this.set = set;
            this.getExpression = getExpression;
            this.setExpression = setExpression;

            this.children = new Lazy<ImmutableDictionary<MemberInfo, IDescendant<TRoot>>>(() => ImmutableDictionary.CreateRange(this.CreateChildren(children)));
        }

        ITypeDescriptor IDescendant.RootType
        {
            get { return this.rootType; }
        }

        public ITypeDescriptor<TRoot> RootType
        {
            get { return this.rootType; }
        }

        ITypeDescriptor IDescendant.NodeType
        {
            get { return this.nodeType; }
        }

        public ITypeDescriptor<TNode> NodeType
        {
            get { return this.nodeType; }
        }

        Expression IDescendant.GetExpression
        {
            get { return this.getExpression; }
        }

        public Expression<Func<TRoot, TNode>> GetExpression
        {
            get { return this.getExpression; }
        }

        Expression IDescendant.SetExpression
        {
            get { return this.setExpression; }
        }

        public Expression<Func<TRoot, TNode, TRoot>> SetExpression
        {
            get { return this.setExpression; }
        }

        public Func<TRoot, TNode> Get
        {
            get { return this.get; }
        }

        public Func<TRoot, TNode, TRoot> Set
        {
            get { return this.set; }
        }

        protected ImmutableDictionary<MemberInfo, IDescendant<TRoot>> Children
        {
            get { return this.children.Value; }
        }

        void IDescendant<TRoot>.Accept(IGraphNodeChildrenVisitor<TRoot> visitor)
        {
            visitor.Visit<TNode>(this.Children);
        }

        public IEnumerable<IDescendant<TRoot>> GetChildren()
        {
            return this.Children.Values;
        }

        public IEnumerable<IDescendant<TRoot, T>> GetChildren<T>()
        {
            return this.Children.Values.OfType<IDescendant<TRoot, T>>();
        }

        public IDescendant<TRoot, T> GetDescendant<T>(Expression<Func<TNode, T>> propertyExpr)
        {
            var members = Unroll(propertyExpr).Reverse().GetEnumerator();

            return this.GetDescendant<T>(members);
        }

        public IEnumerable<IDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>()
        {
            return GetDescendantsInternal<TDescendant>(this, ImmutableHashSet.Create<ITypeDescriptor>());
        }

        public IEnumerable<IDescendant<TRoot, T>> GetDescendants<T>(Func<IDescendant<TRoot>, IEnumerable<IDescendant<TRoot>>> getChildren)
        {
            throw new NotImplementedException();
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

        protected static IEnumerable<IDescendant<TRoot, TDescendant>> GetDescendantsInternal<TDescendant>(IDescendant<TRoot> descendant, IImmutableSet<ITypeDescriptor> seenTypes)
        {
            if (seenTypes.Contains(descendant.NodeType))
            {
                throw new Exception("Cyclic graph detected");
            }

            var foo = new Foo<TRoot>();

            foreach (var c in foo.GetChildren(descendant))
            {
                if (c is IDescendant<TRoot, TDescendant>)
                {
                    yield return (IDescendant<TRoot, TDescendant>)c;
                }

                foreach (var d in GetDescendantsInternal<TDescendant>(c, seenTypes.Add(descendant.NodeType)))
                {
                    yield return d;
                }
            }
        }

        protected IDescendant<TRoot, T> GetDescendant<T>(IEnumerator<MemberInfo> members)
        {
            IDescendant<TRoot> descendant = this;

            var visitor = new ChildFinder<TRoot>();

            while (members.MoveNext())
            {
                descendant = visitor.TryGetChild(descendant, members.Current);
            }

            return descendant as IDescendant<TRoot, T>;
        }
        
        protected IEnumerable<KeyValuePair<MemberInfo, IDescendant<TRoot>>> CreateChildren(IEnumerable<IPropertyDescriptor<TNode>> props)
        {
            var builder = new DescendantBuilder<TRoot, TNode>();

            foreach (var p in props)
            {
                var key = p.ClrMember;
                var value = builder.Build(this, p);

                yield return new KeyValuePair<MemberInfo, IDescendant<TRoot>>(key, value);
            }
        }
    }

    public class Foo<TRoot> : IGraphNodeChildrenVisitor<TRoot>
    {
        private IEnumerable<IDescendant<TRoot>> children;

        public void Visit<TNode>(ImmutableDictionary<MemberInfo, IDescendant<TRoot>> children)
        {
            this.children = children.Values;
        }

        public IEnumerable<IDescendant<TRoot>> GetChildren(IDescendant<TRoot> desc)
        {
            this.children = null;

            desc.Accept(this);

            return this.children;
        }
    }
}