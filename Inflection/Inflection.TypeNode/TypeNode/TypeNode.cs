namespace Inflection.TypeNode.TypeNode
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Immutable.Extensions;
    using Immutable.Monads;
    using Immutable.TypeSystem;

    public static class TypeNode
    {
        public static TypeNode<TNode, TNode> Create<TNode>(IInflector inflector)
        {
            var t = inflector.Inflect<TNode>();

            return new TypeNode<TNode, TNode>(t, x => x, (x, y) => y);
        }
    }

    public class TypeNode<TParent, TNode> : ITypeNode<TParent, TNode>
    {
        private readonly IImmutableType<TNode> nodeType;
        private readonly Func<TParent, TNode> get;
        private readonly IMaybe<Func<TParent, TNode, TParent>> set;
        private readonly ITypePath<TParent, TNode> path;
        private readonly Lazy<Dictionary<MemberInfo, ITypeNode>> children;

        public TypeNode(IImmutableType<TNode> nodeType, Func<TParent, TNode> get, Func<TParent, TNode, TParent> set)
            : this(nodeType, get, Maybe.Return(set))
        {
        }

        public TypeNode(IImmutableType<TNode> nodeType, Func<TParent, TNode> get, IMaybe<Func<TParent, TNode, TParent>> set)
        {
            this.nodeType = nodeType;
            this.get = get;
            this.set = set;

            this.path = TypePath.Create(get, set);

            var visitor = new ChildBuilder<TNode>();
            this.children = new Lazy<Dictionary<MemberInfo, ITypeNode>>(() => this.NodeType.GetProperties().ToDictionary(x => x.ClrMember, visitor.BuildChild));
        }

        IImmutableType ITypeNode.NodeType
        {
            get { return this.NodeType; }
        }

        public IImmutableType<TNode> NodeType
        {
            get { return this.nodeType; }
        }

        public ITypePath<TParent, TNode> Path
        {
            get { return this.path; }
        }

        public Func<TParent, TNode> Get
        {
            get { return this.get; }
        }

        public IMaybe<Func<TParent, TNode, TParent>> Set
        {
            get { return this.set; }
        }

        IMaybe<ITypeNode> ITypeNode.MaybeGetChild(MemberInfo memberInfo)
        {
            return this.MaybeGetChild(memberInfo);
        }

        IEnumerable<ITypeNode> ITypeNode.GetChildren()
        {
            return this.GetChildren();
        }

        IEnumerable<ITypeNode<T>> ITypeNode.GetChildren<T>()
        {
            return this.GetChildren<T>();
        }

        void ITypeNode.Accept(ITypeNodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IMaybe<ITypeNode> MaybeGetChild(MemberInfo memberInfo)
        {
            return Maybe.Return(this.children.Value.GetValueOrDefault(memberInfo));
        }

        public IEnumerable<ITypeNode> GetChildren()
        {
            return this.children.Value.Values;
        }

        public IEnumerable<ITypeNode<TNode, T>> GetChildren<T>()
        {
            return this.GetChildren().OfType<ITypeNode<TNode, T>>();
        }

        public IMaybe<ITypeNode> MaybeGetNode(IEnumerable<MemberInfo> memberInfos)
        {
            var mn = Maybe.Return(this as ITypeNode);

            foreach (var info in memberInfos)
            {
                var info1 = info;

                mn = mn.Bind(n => n.MaybeGetChild(info1));
            }

            return mn;
        }

        public IEnumerable<ITypeNode> GetNodes()
        {
            var q = new Queue<ITypeNode>();

            foreach (var c in this.GetChildren())
            {
                yield return c;

                q.Enqueue(c);
            }

            while (q.Count > 0)
            {
                var d = q.Dequeue();

                foreach (var c in d.GetChildren())
                {
                    yield return c;

                    q.Enqueue(c);
                }
            }
        }

        public IEnumerable<ITypeNode<T>> GetNodes<T>()
        {
            return this.GetNodes().OfType<ITypeNode<T>>();
        }

        public IMaybe<ITypePath<TNode, T>> MaybeGetPath<T>(IEnumerable<MemberInfo> memberInfos)
        {
            var nodes = new List<ITypeNode>();

            var mn = Maybe.Return(this as ITypeNode);

            foreach (var info in memberInfos)
            {
                var info1 = info;

                mn = mn.Bind(
                             n =>
                             {
                                 nodes.Add(n);
                                 return n.MaybeGetChild(info1);
                             });

                if (!(mn is Just<ITypeNode>))
                {
                    return new Nothing<ITypePath<TNode, T>>();
                }
            }

            var pathBuilder = new PathBuilder<TNode, T>();

            return pathBuilder.MaybeBuild(nodes);
        }

        public IEnumerable<ITypePath<TNode, T>> GetPaths<T>()
        {
            return GetPaths<T>(new PathStack<TNode>(), this.GetChildren());
        }

        public IEnumerable<IValuePath<TNode, T>> GetValuePaths<T>(TNode node)
        {
            return GetValuePaths<T>(new ValuePathStack<TNode>(node), this.GetChildren());
        }

        public IEnumerable<T> GetValues<T>(TNode node)
        {
            return GetValues<T>(new ValueStack<TNode>(node), this.GetChildren());
        }

        private static IEnumerable<ITypePath<TNode, T>> GetPaths<T>(PathStack<TNode> stack, IEnumerable<ITypeNode> children)
        {
            foreach (var child in children)
            {
                stack.Push(child);

                foreach (var p in GetPaths<T>(stack, child.GetChildren()))
                {
                    yield return p;
                }

                var d = stack.Pop() as ITypePath<TNode, T>;
                if (d != null)
                {
                    yield return d;
                }
            }
        }

        private static IEnumerable<IValuePath<TNode, T>> GetValuePaths<T>(ValuePathStack<TNode> stack, IEnumerable<ITypeNode> children)
        {
            foreach (var child in children)
            {
                stack.Push(child);

                var top = stack.Peek();
                if (top.IsNull)
                {
                    stack.Pop();
                    continue;
                }

                var d = top as IValuePath<TNode, T>;
                if (d != null)
                {
                    yield return d;
                }
                
                foreach (var p in GetValuePaths<T>(stack, child.GetChildren()))
                {
                    yield return p;
                }

                stack.Pop();
            }
        }

        private static IEnumerable<T> GetValues<T>(ValueStack<TNode> stack, IEnumerable<ITypeNode> children)
        {
            foreach (var child in children)
            {
                stack.Push(child);

                var top = stack.Peek();
                if (top == null)
                {
                    stack.Pop();
                    continue;
                }

                if (top is T)
                {
                    yield return (T)top;
                }

                foreach (var p in GetValues<T>(stack, child.GetChildren()))
                {
                    yield return p;
                }

                stack.Pop();
            }
        }

        private static T MapIfCompatible<T, TA, TB>(T v, Func<TA, TB> f) {
            return (v is TA && typeof(T).IsAssignableFrom(typeof(TB)))
                ? (T)(object)f((TA)(object)v)
                : v;
        }

        public TNode GMap<TA, TB>(TNode node, Func<TA, TB> f) {
            node = MapIfCompatible(node, f);

            foreach (var p in this.GetPaths<TA>()) {
                var set = p.Set.GetValueOrDefault();
                if (set == null) {
                    continue;
                }

                var prop = MapIfCompatible(p.Get(node), f);
                node = set(node, prop);
            }

            return node;
        }
    }
}