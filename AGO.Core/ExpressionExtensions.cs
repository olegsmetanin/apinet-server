using System;
using System.Linq;
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
				var methodCallExpr = unaryExpr.Operand as MethodCallExpression;
				if (methodCallExpr != null)
				{
					var compiled = Expression.Lambda(methodCallExpr).Compile();
					var value = compiled.DynamicInvoke();
					constExpr = Expression.Constant(value);
				}
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

		//http://stackoverflow.com/questions/729295/how-to-cast-expressionfunct-datetime-to-expressionfunct-object
		public static Expression<Func<TModel, TToProperty>> Cast<TModel, TFromProperty, TToProperty>(this Expression<Func<TModel, TFromProperty>> expression)
		{
			Expression converted = Expression.Convert(expression.Body, typeof(TToProperty));

			return Expression.Lambda<Func<TModel, TToProperty>>(converted, expression.Parameters);
		}
	}
}
