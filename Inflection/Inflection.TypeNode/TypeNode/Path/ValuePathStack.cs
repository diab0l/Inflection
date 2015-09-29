namespace Inflection.TypeNode.TypeNode {
    using System;
    using System.Collections.Generic;

    public class ValuePathStack<TNode> : ITypeNodeVisitor {
        private readonly Stack<IValue> pathSt = new Stack<IValue>();
        private IValue top;

        public ValuePathStack(TNode root) {
            this.top = ValuePath.Create((TNode x) => x, (x, y) => y, root);
            this.pathSt.Push(this.top);
        }

        public void Push(ITypeNode node) {
            node.Accept(this);
        }

        public IValue Peek() {
            return this.top;
        }

        public IValue Pop() {
            var pop = this.pathSt.Pop();
            this.top = this.pathSt.Peek();
            return pop;
        }

        void ITypeNodeVisitor.Visit<TVisiteeParent, TVisiteeNode>(ITypeNode<TVisiteeParent, TVisiteeNode> visitee) {
            var top = this.top as IValuePath<TNode, TVisiteeParent>;
            if (top == null) {
                throw new Exception("Type mismatch");
            }

            this.top = top.Wrap(visitee.Get, visitee.Set, visitee.Get(top.Value));
            this.pathSt.Push(this.top);
        }
    }
}