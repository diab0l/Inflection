namespace Inflection.Immutable.Graph.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Monads;

    public class ChildFinderVisitor<TRoot> : ITypeDescendantChildrenVisitor<TRoot>
    {
        private MemberInfo member;
        private IMaybe<ITypeDescendant<TRoot>> child;

        public IMaybe<ITypeDescendant<TRoot>> MaybeGetChild(ITypeDescendant<TRoot> node, MemberInfo member)
        {
            this.member = member;
            node.Accept(this);

            var result = this.child;
            this.child = null;
            return result;
        }

        public void Visit<TNode>(Func<MemberInfo, IMaybe<ITypeDescendant<TRoot>>> findChild, IEnumerable<ITypeDescendant<TRoot>> children)
        {
            this.child = findChild(this.member);
        }
    }
}