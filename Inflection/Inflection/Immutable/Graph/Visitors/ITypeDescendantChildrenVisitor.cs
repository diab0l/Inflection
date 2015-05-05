namespace Inflection.Immutable.Graph.Visitors
{
    using System;
    using System.Reflection;

    using Monads;

    public interface ITypeDescendantChildrenVisitor
    {
        void Visit<TRoot, TNode>(Func<MemberInfo, IMaybe<ITypeDescendant<TRoot>>> findChild);
    }

    public interface ITypeDescendantChildrenVisitor<TRoot>
    {
        void Visit<TNode>(Func<MemberInfo, IMaybe<ITypeDescendant<TRoot>>> findChild);
    }
}