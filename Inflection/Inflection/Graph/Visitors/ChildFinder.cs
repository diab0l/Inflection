namespace Inflection.Graph.Visitors
{
    using System.Collections.Immutable;
    using System.Reflection;

    public class ChildFinder<TRoot> : IGraphNodeChildrenVisitor<TRoot>
    {
        private MemberInfo member;
        private IDescendant<TRoot> result;

        public IDescendant<TRoot> TryGetChild(IDescendant<TRoot> node, MemberInfo member)
        {
            this.member = member;
            node.Accept(this);

            var result = this.result;
            this.result = null;
            return result;
        }

        public void Visit<TNode>(ImmutableDictionary<MemberInfo, IDescendant<TRoot>> children)
        {
            children.TryGetValue(this.member, out this.result);
        }
    }
}