namespace Inflection.Immutable.Graph.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Monads;

    public class ChildExtractionVisitor<TRoot> : ITypeDescendantChildrenVisitor<TRoot>
    {
        private IEnumerable<ITypeDescendant<TRoot>> children;

        public void Visit<TNode>(Func<MemberInfo, IMaybe<ITypeDescendant<TRoot>>> findChild, IEnumerable<ITypeDescendant<TRoot>> children)
        {
            this.children = children;
        }

        public IEnumerable<ITypeDescendant<TRoot>> GetChildren(ITypeDescendant<TRoot> desc)
        {
            this.children = null;

            desc.Accept(this);

            return this.children;
        }
    }
}