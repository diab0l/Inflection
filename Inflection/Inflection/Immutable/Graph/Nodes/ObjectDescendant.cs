namespace Inflection.Immutable.Graph.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Monads;

    using TypeSystem;
    using TypeSystem.Visitors;

    using Visitors;

    public class ObjectDescendant<TRoot, TNode> : IObjectDescendant<TRoot, TNode>
    {
        private readonly IMaybe<IObjectDescendant<TRoot>> parent;
        private readonly IMaybe<IImmutableProperty> property; 
        private readonly IImmutableType<TNode> nodeType; 

        private readonly TNode value;
        private readonly IMaybe<Func<TNode, TRoot>> set;
        
        public ObjectDescendant(IMaybe<IObjectDescendant<TRoot>> parent, IMaybe<IImmutableProperty> property, IImmutableType<TNode> nodeType, IMaybe<Func<TNode, TRoot>> set, TNode value)
        {
            this.parent = parent;
            this.property = property;
            this.nodeType = nodeType;
            this.value = value;

            this.set = set;

            //this.set = this.TypeDescendant
            //               .Set
            //               .Apply(this.MaybeSet);
        }

        public static ObjectDescendant<TRoot, TNode> Create<TParentNode>(
            IObjectDescendant<TRoot, TParentNode> parent,
            IImmutableProperty<TParentNode, TNode> property,
            TNode value)
        {
            var set = parent.Set.Apply(x => property.Set.Transform(y => new Func<TNode, TRoot>(tn => x(y(parent.Value, tn)))));

            return new ObjectDescendant<TRoot, TNode>(Maybe.Return(parent), Maybe.Return(property), property.PropertyType, set, value);
        }

        public IImmutableType<TNode> NodeType {
            get { return this.nodeType; }
        }

        IImmutableType IObjectDescendant.NodeType
        {
            get { return this.nodeType; }
        }

        public TNode Value
        {
            get { return this.value; }
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
                          .Apply(x => Maybe.Return(x as IObjectDescendant<TRoot, TDescendant>));
        }

        public IEnumerable<IObjectDescendant<TRoot, TDescendant>> GetDescendants<TDescendant>()
        {
            var childrenBuilder = new ChildrenBuilderVisitor<TRoot, TNode>();

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

        public ITypeDescendant<TRoot, TNode> Open()
        {
            // TODO: use the parents, properties and a visitor to construct the type descendant

            throw new NotImplementedException();
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

    public class ChildrenBuilderVisitor<TRoot, TDeclaring> : IImmutablePropertyVisitor<TDeclaring>
    {
        private IObjectDescendant<TRoot, TDeclaring> parent;

        private IObjectDescendant<TRoot> descendant;

#pragma warning disable 183
        public void Visit<TProperty>(IImmutableProperty<TDeclaring, TProperty> prop)
        {
            var value = prop.Get(this.parent.Value);

            if (!(value is TProperty))
            {
                return;
            }

            this.descendant = ObjectDescendant<TRoot, TProperty>.Create(this.parent, prop, value);
        }
#pragma warning restore 183

        public IObjectDescendant<TRoot> GetChild(IObjectDescendant<TRoot, TDeclaring> parent, IImmutableProperty<TDeclaring> property)
        {
            this.parent = parent;

            this.descendant = null;
            property.Accept(this);
            return this.descendant;
        }
    }
}
