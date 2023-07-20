using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FirestoreExtensions
{
    public static class PropertyPathProvider<T> where T : class
    {
        public static string GetPropertyPath<P>(Expression<Func<T, P>> expr)
        {
            MemberExpression me;
            switch (expr.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var ue = expr.Body as UnaryExpression;
                    me = ((ue != null) ? ue.Operand : null) as MemberExpression;
                    break;

                default:
                    me = expr.Body as MemberExpression;
                    break;
            }

            var parts = new List<string>();

            while (me != null)
            {
                parts.Add(me.Member.Name);
                me = me.Expression as MemberExpression;
            }

            parts.Reverse();
            return string.Join(".", parts);
        }
    }
}