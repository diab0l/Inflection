namespace Inflection.OpenGraph.Visitors
{
    using System.Collections.Immutable;
    using System.Reflection;

    public interface IGraphNodeChildrenVisitor<TRoot>
    {
        void Visit<TNode>(ImmutableDictionary<MemberInfo, ITypeDescendant<TRoot>> children);
    }
}