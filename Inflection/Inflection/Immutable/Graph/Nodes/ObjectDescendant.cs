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

    public class ObjectDescendant<TRoot, TNode> : IObjectDescendant<TRoot, TNode>
    {
        private readonly IMaybe<IObjectDescendant<TRoot>> parent;
        private readonly IMaybe<IImmutableProperty> property; 
        private readonly IImmutableType<TNode> nodeType; 

        private readonly TNode value;
        private readonly IMaybe<Func<TNode, TRoot>> set;

        private readonly Expression<Func<TRoot, TNode>> getExpression;

        public ObjectDescendant(IMaybe<IObjectDescendant<TRoot>> parent, IMaybe<IImmutableProperty> property, IImmutableType<TNode> nodeType, IMaybe<Func<TNode, TRoot>> set, TNode value, Expression<Func<TRoot, TNode>> getExpression)
        {
            this.parent = parent;
            this.property = property;
            this.nodeType = nodeType;
            this.value = value;
            this.getExpression = getExpression;

            this.set = set;
        }

        public static ObjectDescendant<TRoot, TNode> Create<TParentNode>(
            IObjectDescendant<TRoot, TParentNode> parent,
            IImmutableProperty<TParentNode, TNode> property,
            TNode value)
        {
            var set = parent.Set.Bind(x => property.Set.FMap(y => new Func<TNode, TRoot>(tn => x(y(parent.Value, tn)))));

            var parentGet = parent.GetExpression;
            var propertyGet = property.GetExpression;

            var getExpr = Expression.Lambda<Func<TRoot, TNode>>(propertyGet.Body.Replace(propertyGet.Parameters[0], parentGet.Body), parentGet.Parameters);

            return new ObjectDescendant<TRoot, TNode>(Maybe.Return(parent), Maybe.Return(property), property.PropertyType, set, value, getExpr);
        }

        public IImmutableType<TNode> NodeType {
            get { return this.nodeType; }
        }

        public IMaybe<IObjectDescendant<TRoot>> Parent
        {
            get { return this.parent; }
        }

        public IMaybe<IImmutableProperty> Property
        {
            get { return this.property; }
        }

        IMaybe<IObjectDescendant> IObjectDescendant.Parent
        {
            get { return this.parent; }
        }

        IImmutableType IObjectDescendant.NodeType
        {
            get { return this.nodeType; }
        }

        public TNode Value
        {
            get { return this.value; }
        }

        public Expression<Func<TRoot, TNode>> GetExpression
        {
            get { return this.getExpression; }
        }

        public IMaybe<Func<TNode, TRoot>> Set { get { return this.set; } }

        public IEnumerable<IObjectDescendant<TRoot, TDescendant>> GetChildren<TDescendant>()
        {
            var childProps = this.NodeType.GetProperties<TDescendant>();

            foreach (var prop in childProps)
            {
                var v = prop.Get(this.Value);

                yield return ObjectDescendant<TRoot, TDescendant>.Create(this, prop, v);
            }
        }

        public IMaybe<IObjectDescendant<TRoot, TDescendant>> GetDescendant<TDescendant>(Expression<Func<TNode, TDescendant>> memExpr)
        {
            var path = Unroll(memExpr).Reverse();
            
            var visitor = new GetObjectDescendantVisitor<TRoot, TNode>(this);

            return visitor.MaybeGetDescendant(ImmutableQueue.CreateRange(path))
                          .Bind(x => Maybe.Return(x as IObjectDescendant<TRoot, TDescendant>));
        }

        public IEnumerable<IObjectDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>()
        {
            var childrenBuilder = new ObjectDescendantBuilderVisitor<TRoot, TNode>();

            var childProps = this.NodeType.GetProperties();

            foreach (var cp in childProps)
            {
                var child = childrenBuilder.GetChild(this, cp);

                if (child == null)
                {
                    continue;
                }

                if (child is IObjectDescendant<TRoot, TDescendant>)
                {
                    yield return (IObjectDescendant<TRoot, TDescendant>)child;
                }

                foreach (var c in child.GetDescendants<TDescendant>())
                {
                    yield return c;
                }
            }
        }

        public IMaybe<ITypeDescendant<TRoot, TNode>> Open()
        {
            var tg = TypeGraph.Create<TRoot>(this.NodeType.Inflector);

            return tg.GetDescendant(this.getExpression);
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
    }
}
