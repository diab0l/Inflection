namespace Inflection.Immutable.Extensions
{
    using System.Linq;
    using System.Linq.Expressions;

    public static class ExpressionExtensions
    {
        public static Expression Replace(this Expression @this, Expression old, Expression @new)
        {
            if (@this == old)
            {
                return @new;
            }

            if (@this is BinaryExpression)
            {
                var expr = @this as BinaryExpression;
                var left = Replace(expr.Left, old, @new);
                var right = Replace(expr.Right, old, @new);

                return Expression.MakeBinary(expr.NodeType, left, right, expr.IsLiftedToNull, expr.Method, expr.Conversion);
            }

            if (@this is BlockExpression)
            {
                var expr = @this as BlockExpression;

                var exprs = expr.Expressions.Select(x => Replace(x, old, @new));

                return expr.Update(expr.Variables, exprs);
            }

            if (@this is ConditionalExpression)
            {
                var expr = @this as ConditionalExpression;

                var test = Replace(expr.Test, old, @new);
                var ifTrue = Replace(expr.IfTrue, old, @new);
                var ifFalse = Replace(expr.IfFalse, old, @new);

                return expr.Update(test, ifTrue, ifFalse);
            }

            if (@this is DynamicExpression)
            {
                var expr = @this as DynamicExpression;

                var args = expr.Arguments.Select(x => x.Replace(old, @new));

                return expr.Update(args);
            }

            if (@this is GotoExpression)
            {
                var expr = @this as GotoExpression;

                var value = expr.Value.Replace(old, @new);

                return expr.Update(expr.Target, value);
            }

            if (@this is IndexExpression)
            {
                var expr = @this as IndexExpression;

                var obj = expr.Object.Replace(old, @new);
                var args = expr.Arguments.Select(x => x.Replace(old, @new));

                return expr.Update(obj, args);
            }

            if (@this is InvocationExpression)
            {
                var expr = @this as InvocationExpression;

                var x = expr.Expression.Replace(old, @new);
                var args = expr.Arguments.Select(y => y.Replace(old, @new));

                return expr.Update(x, args);
            }

            if (@this is LabelExpression)
            {
                var lExpr = @this as LabelExpression;
                var dValue = Replace(lExpr.DefaultValue, old, @new);

                return lExpr.Update(lExpr.Target, dValue);
            }

            if (@this is LambdaExpression)
            {
                var expr = @this as LambdaExpression;

                var body = expr.Body.Replace(old, @new);
                var name = expr.Name;
                var tailCall = expr.TailCall;
                var pars = expr.Parameters;

                return Expression.Lambda(body, name, tailCall, pars);
            }

            if (@this is ListInitExpression)
            {
                var expr = @this as ListInitExpression;

                var nExpr = (NewExpression)expr.NewExpression.Replace(old, @new);
                var args = expr.Initializers.Select(x => x.Update(x.Arguments.Select(y => y.Replace(old, @new))));

                return expr.Update(nExpr, args);
            }

            if (@this is LoopExpression)
            {
                var expr = @this as LoopExpression;

                var body = expr.Body.Replace(old, @new);

                return expr.Update(expr.BreakLabel, expr.ContinueLabel, body);
            }

            if (@this is MemberExpression)
            {
                var mExpr = @this as MemberExpression;

                var expr = Replace(mExpr.Expression, old, @new);

                return mExpr.Update(expr);
            }
           
            if (@this is MemberInitExpression)
            {
                var expr = @this as MemberInitExpression;

                var nExpr = (NewExpression)expr.NewExpression.Replace(old, @new);

                return expr.Update(nExpr, expr.Bindings);
            }

            if (@this is MethodCallExpression)
            {
                var expr = @this as MethodCallExpression;

                var x = expr.Object.Replace(old, @new);
                var args = expr.Arguments.Select(y => y.Replace(old, @new));

                return expr.Update(x, args);
            }

            if (@this is NewExpression)
            {
                var expr = @this as NewExpression;

                var args = expr.Arguments.Select(x => x.Replace(old, @new));

                return expr.Update(args);
            }

            if (@this is NewArrayExpression)
            {
                var expr = @this as NewArrayExpression;

                var args = expr.Expressions.Select(x => x.Replace(old, @new));

                return expr.Update(args);
            }

            if (@this is RuntimeVariablesExpression)
            {
                var expr = @this as RuntimeVariablesExpression;

                var ps = expr.Variables.Select(x => x.Replace(old, @new)).OfType<ParameterExpression>();

                return expr.Update(ps);
            }

            if (@this is SwitchExpression)
            {
                var expr = @this as SwitchExpression;

                var v = expr.SwitchValue.Replace(old, @new);
                var cs = expr.Cases.Select(x => x.Update(x.TestValues.Select(y => y.Replace(old, @new)), x.Body.Replace(old, @new)));
                var b = expr.DefaultBody.Replace(old, @new);

                return expr.Update(v, cs, b);
            }

            if (@this is TryExpression)
            {
                var expr = @this as TryExpression;

                var body = expr.Body.Replace(old, @new);
                var hs = expr.Handlers.Select(x => x.Update((ParameterExpression)x.Variable.Replace(old, @new), x.Filter.Replace(old, @new), x.Body.Replace(old, @new)));
                var fin = expr.Finally.Replace(old, @new);
                var flt = expr.Fault.Replace(old, @new);

                return expr.Update(body, hs, fin, flt);
            }

            if (@this is TypeBinaryExpression)
            {
                var expr = @this as TypeBinaryExpression;

                return expr.Update(expr.Expression.Replace(old, @new));
            }

            if (@this is UnaryExpression)
            {
                var uExpr = @this as UnaryExpression;

                var op = Replace(uExpr.Operand, old, @new);

                return uExpr.Update(op);
            }

            return @this;
        }
    }
}