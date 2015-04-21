namespace Inflection.Immutable.Graph.Visitors
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    using Immutable;

    using Monads;

    using Nodes;

    using TypeSystem;
    using TypeSystem.Visitors;

    public class GetObjectDescendantVisitor<TRoot, TNode> : IImmutablePropertyVisitor<TNode>
    {
        private readonly IObjectDescendant<TRoot, TNode> parent;
        private ImmutableQueue<MemberInfo> members;
        
        private IMaybe<IObjectDescendant<TRoot>> child = new Nothing<IObjectDescendant<TRoot>>();

        public GetObjectDescendantVisitor(IObjectDescendant<TRoot, TNode> parent)
        {
            this.parent = parent;
        }

#pragma warning disable 183
        void IImmutablePropertyVisitor<TNode>.Visit<TProperty>(IImmutableProperty<TNode, TProperty> prop)
        {
            var value = prop.Get(this.parent.Value);

            if (!(value is TProperty))
            {
                return;
            }

            var child = ObjectDescendant<TRoot, TProperty>.Create(this.parent, prop, value);

            if (this.members.IsEmpty)
            {
                this.child = Maybe.Return<IObjectDescendant<TRoot>>(child);
                return;
            }

            var foo = new GetObjectDescendantVisitor<TRoot, TProperty>(child);
            this.child = foo.MaybeGetDescendant(this.members.Dequeue());
        }
#pragma warning restore 183

        public IMaybe<IObjectDescendant<TRoot>> MaybeGetDescendant(ImmutableQueue<MemberInfo> members)
        {
            if (members.IsEmpty)
            {
                return this.child;
            }

            var mem = members.Peek();
            this.members = members.Dequeue();

            var type = this.parent.NodeType;

            var visitor = new FindPropertyVisitor<TNode>();
            return visitor.MaybeFindChild(type, mem)
                          .Bind(
                                 x =>
                                 {
                                     x.Accept(this);
                                     return this.child;
                                 });
        }
    }

    public class FindPropertyVisitor<TDeclaring> : IImmutableTypePropertiesVisitor<TDeclaring>
    {
        private IMaybe<IImmutableProperty<TDeclaring>> property;

        private MemberInfo member;

        public void Visit(Func<MemberInfo, IMaybe<IImmutableProperty<TDeclaring>>> findChild)
        {
            this.property = findChild(this.member);
        }

        public IMaybe<IImmutableProperty<TDeclaring>> MaybeFindChild(IImmutableType<TDeclaring> type, MemberInfo member)
        {
            this.member = member;
            
            this.property = new Nothing<IImmutableProperty<TDeclaring>>();
            type.Accept(this);
            return this.property;
        }
    }
}