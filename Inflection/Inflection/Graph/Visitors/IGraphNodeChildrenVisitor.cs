namespace Inflection.Graph.Visitors
{
    using System.Collections.Immutable;
    using System.Reflection;

    public interface IGraphNodeChildrenVisitor<TRoot>
    {
        void Visit<TNode>(ImmutableDictionary<MemberInfo, IDescendant<TRoot>> children);
    }
}