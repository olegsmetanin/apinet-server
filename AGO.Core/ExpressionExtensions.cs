using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AGO.Core
{
	public static class ExpressionExtensions
	{
		public static object ValueFromExpression(this Expression expr)
		{
			var constExpr = expr as ConstantExpression;
			var memberExpr = expr as MemberExpression;
			var unaryExpr = expr as UnaryExpression;

			if (unaryExpr != null)
			{
				memberExpr = unaryExpr.Operand as MemberExpression;
				constExpr = unaryExpr.Operand as ConstantExpression;
			}

			if (constExpr != null)
				return constExpr.Value;

			if (memberExpr == null)
				throw new UnexpectedExpressionTypeException<MemberExpression>(expr.Type);

			var getter = Expression.Lambda<Func<object>>(Expression.Convert(memberExpr, typeof(object))).Compile();
			return getter();
		}

		public static PropertyInfo PropertyInfoFromExpression(this Expression expression)
		{
			var lambdaExpr = expression as LambdaExpression;
			if (lambdaExpr != null)
				return PropertyInfoFromExpression(lambdaExpr.Body);

			var memberExpression = expression as MemberExpression;
			var unaryExpr = expression as UnaryExpression;

			if (unaryExpr != null)
				memberExpression = unaryExpr.Operand as MemberExpression;

			if (memberExpression == null)
				throw new UnexpectedExpressionTypeException<MemberExpression>(expression.Type);

			var result = memberExpression.Member as PropertyInfo;
			if (result == null)
				throw new PropertyAccessExpressionExpectedException();

			return result;
		}
	}
}
