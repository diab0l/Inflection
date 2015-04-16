namespace Inflection.OpenGraph.Visitors
{
    using System.Collections.Immutable;
    using System.Reflection;

    public class ChildFinder<TRoot> : IGraphNodeChildrenVisitor<TRoot>
    {
        private MemberInfo member;
        private ITypeDescendant<TRoot> result;

        public ITypeDescendant<TRoot> TryGetChild(ITypeDescendant<TRoot> node, MemberInfo member)
        {
            this.member = member;
            node.Accept(this);

            var result = this.result;
            this.result = null;
            return result;
        }

        public void Visit<TNode>(ImmutableDictionary<MemberInfo, ITypeDescendant<TRoot>> children)
        {
            children.TryGetValue(this.member, out this.result);
        }
    }
}