namespace Inflection.TypeNode.TypeNode {
    using System.Collections.Generic;

    using Immutable.Monads;

    public class PathBuilder<TNode, T> : ITypeNodeVisitor {
        private ITypePath path;

        public IMaybe<ITypePath<TNode, T>> MaybeBuild(IEnumerable<ITypeNode> nodes) {
            this.path = TypePath.Create<TNode, TNode>(x => x, (x, y) => y);

            foreach (var node in nodes) {
                if (this.path == null) {
                    return new Nothing<ITypePath<TNode, T>>();
                }

                node.Accept(this);
            }

            return Maybe.Return(this.path as ITypePath<TNode, T>);
        }

        void ITypeNodeVisitor.Visit<TVisiteeParent, TVisiteeNode>(ITypeNode<TVisiteeParent, TVisiteeNode> visitee) {
            var mPrev = this.path as ITypePath<TNode, TVisiteeParent>;

            if (mPrev == null) {
                this.path = null;
                return;
            }

            this.path = mPrev.Wrap(visitee.Get, visitee.Set);
        }
    }
}