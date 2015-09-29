namespace Inflection.TypeNode.TypeNode {
    using System;
    using System.Collections.Generic;

    public class PathStack<TNode> : ITypeNodeVisitor {
        private readonly Stack<ITypePath> pathSt = new Stack<ITypePath>();
        private ITypePath path;

        public void Push(ITypeNode node) {
            if (this.pathSt.Count == 0) {
                var bottom = TypePath.Create<TNode, TNode>(x => x, (x, y) => y);
                this.pathSt.Push(bottom);
            }

            this.path = null;
            node.Accept(this);
            this.pathSt.Push(this.path);
        }

        public ITypePath Pop() {
            return this.pathSt.Pop();
        }

        public void Visit<TVisiteeParent, TVisiteeNode>(ITypeNode<TVisiteeParent, TVisiteeNode> visitee) {
            var top = this.pathSt.Peek() as ITypePath<TNode, TVisiteeParent>;
            if (top == null) {
                throw new Exception("Type mismatch");
            }

            this.path = top.Wrap(visitee.Get, visitee.Set);
        }
    }
}