namespace Inflection.TypeNode.TypeNode {
    using System;
    using System.Collections.Generic;

    public class ValueStack<TNode> : ITypeNodeVisitor {
        private readonly Stack<object> pathSt = new Stack<object>();

        public ValueStack(TNode root) {
            this.pathSt.Push(root);
        }

        public void Push(ITypeNode node) {
            node.Accept(this);
        }

        public object Peek() {
            return this.pathSt.Peek();
        }

        public void Pop() {
            this.pathSt.Pop();
        }

        void ITypeNodeVisitor.Visit<TVisiteeParent, TVisiteeNode>(ITypeNode<TVisiteeParent, TVisiteeNode> visitee) {
            var top = this.Peek();
            if (!(top is TVisiteeParent)) {
                throw new Exception("Type mismatch");
            }

            this.pathSt.Push(visitee.Get((TVisiteeParent)top));
        }
    }
}