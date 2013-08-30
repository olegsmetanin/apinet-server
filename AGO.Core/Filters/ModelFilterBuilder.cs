using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AGO.Core.Model;

namespace AGO.Core.Filters
{
	internal class ModelFilterBuilder<TModel, TPrevModel> : ModelFilterNode, IModelFilterBuilder<TModel, TPrevModel>
		where TModel : class, IIdentifiedModel
		where TPrevModel : class, IIdentifiedModel
	{
		#region Properties, fields, constructors

		protected readonly PropertyInfo _PropertyInfo;

		protected readonly IModelFilterNode _PrevBuilder;

		public ModelFilterBuilder(IModelFilterNode prevBuilder, PropertyInfo propertyInfo)
		{
			_PrevBuilder = prevBuilder;

			if (propertyInfo != null)
				Path = propertyInfo.Name;
			_PropertyInfo = propertyInfo;
		}

		#endregion

		#region Interfaces implementation

		public IModelFilterBuilder<TModel, TPrevModel> And()
		{
			var builder = new ModelFilterBuilder<TModel, TPrevModel>(
				this, null) {Operator = ModelFilterOperators.And};
			_Items.Add(builder);

			return builder;
		}

		public IModelFilterBuilder<TModel, TPrevModel> Or()
		{
			var builder = new ModelFilterBuilder<TModel, TPrevModel>(
				this, null) { Operator = ModelFilterOperators.Or };
			_Items.Add(builder);

			return builder;
		}

		public IModelFilterBuilder<TNextModel, TModel> WhereModel<TNextModel>(
			Expression<Func<TModel, TNextModel>> expression)
			where TNextModel : class, IIdentifiedModel
		{
			var propertyInfo = expression.PropertyInfoFromExpression();

			var existing = _Items.FirstOrDefault(filterNode => propertyInfo.Name.Equals(filterNode.Path));
			var existingBuilder = existing as IModelFilterBuilder<TNextModel, TModel>;
			if (existingBuilder != null)
				return existingBuilder;
			if (existing != null)
				throw new FilterAlreadyContainsNodeWithSameNameException(existing.Path);

			var builder = new ModelFilterBuilder<TNextModel, TModel>(this, propertyInfo) { Operator = ModelFilterOperators.And };
			_Items.Add(builder);
			return builder;
		}

		public IModelFilterBuilder<TNextModel, TModel> WhereCollection<TNextModel>(
			Expression<Func<TModel,
			IEnumerable<TNextModel>>> expression)
			where TNextModel : class, IIdentifiedModel
		{
			var propertyInfo = expression.PropertyInfoFromExpression();

			var existing = _Items.FirstOrDefault(filterNode => propertyInfo.Name.Equals(filterNode.Path));
			var existingBuilder = existing as IModelFilterBuilder<TNextModel, TModel>;
			if (existingBuilder != null)
				return existingBuilder;
			if (existing != null)
				throw new FilterAlreadyContainsNodeWithSameNameException(existing.Path);

			var builder = new ModelFilterBuilder<TNextModel, TModel>(this, propertyInfo) { Operator = ModelFilterOperators.And };
			_Items.Add(builder);
			return builder;
		}

		public IModelFilterBuilder<TPrevModel, TPrevModel> End()
		{
			if (_PrevBuilder == null)
				throw new EndJoinWithoutJoinException();

			var result = _PrevBuilder as IModelFilterBuilder<TPrevModel, TPrevModel>;
			if (_PrevBuilder == null)
				throw new DeepEndJoinException();

			return result;
		}

		IModelFilterBuilder<TModel, TPrevModel> IModelFilterBuilder<TModel, TPrevModel>.Where<TValue>(
			Expression<Func<TModel, TValue>> expression)
		{
			ProcessExpression(this, expression.Body, null, null, false);
			return this;
		}

		public IValueFilterBuilder<TModel, TPrevModel, TValue> WhereProperty<TValue>(
			Expression<Func<TModel, TValue>> expression)
		{
			var memberExpression = expression.Body as MemberExpression;
			if (memberExpression == null)
				throw new UnexpectedExpressionTypeException(expression.Body.Type);

			var current = this as IModelFilterNode;
			current = UnwindMemberExpression(current, memberExpression);

			var propertyInfo = expression.PropertyInfoFromExpression();
			var builder = new ValueFilterBuilder<TModel, TPrevModel, TValue>(propertyInfo, this);
			current.AddItem(builder);
			return builder;
		}

		public IStringFilterBuilder<TModel, TPrevModel> WhereString(
			Expression<Func<TModel, string>> expression)
		{
			var memberExpression = expression.Body as MemberExpression;
			if (memberExpression == null)
				throw new UnexpectedExpressionTypeException(expression.Body.Type);

			var current = this as IModelFilterNode;
			current = UnwindMemberExpression(current, memberExpression);

			var propertyInfo = expression.PropertyInfoFromExpression();
			var builder = new StringFilterBuilder<TModel, TPrevModel>(propertyInfo, this);
			current.AddItem(builder);
			return builder;
		}

		#endregion

		#region Helper methods

		protected void ProcessExpression(IModelFilterNode current, Expression expr, ValueFilterOperators? op, object value, bool negative)
		{
			var unaryExpr = expr as UnaryExpression;
			if (unaryExpr != null)
			{
				if (unaryExpr.NodeType != ExpressionType.Not &&
						unaryExpr.NodeType != ExpressionType.Convert && unaryExpr.NodeType != ExpressionType.ConvertChecked)
					throw new UnexpectedExpressionTypeException(expr.Type);
 
				if (unaryExpr.NodeType == ExpressionType.Not)
					negative = !negative;
				ProcessExpression(current, unaryExpr.Operand, op, value, negative);
				return;
			}

			var binaryExpr = expr as BinaryExpression;
			if (binaryExpr != null)
			{
				ProcessBinaryExpression(current, binaryExpr, negative);
				return;
			}

			var memberExpr = expr as MemberExpression;
			if (memberExpr == null)
				throw new UnexpectedExpressionTypeException(expr.Type);

			ProcessMemberExpression(current, memberExpr, op, value, negative);
		}

		protected void ProcessBinaryExpression(IModelFilterNode current, BinaryExpression expr, bool negative)
		{
			if (expr.NodeType == ExpressionType.AndAlso || expr.NodeType == ExpressionType.OrElse)
			{
				var modelFilter = new ModelFilterNode
				{
					Operator = expr.NodeType == ExpressionType.AndAlso
						? ModelFilterOperators.And
						: ModelFilterOperators.Or
				};
				current.AddItem(modelFilter);

				ProcessExpression(modelFilter, expr.Left, null, null, negative);
				ProcessExpression(modelFilter, expr.Right, null, null, negative);
				return;
			}

			var value = expr.Right.ValueFromExpression();

			ValueFilterOperators? op = null;

			if (expr.NodeType == ExpressionType.Equal)
				op = value == null ? ValueFilterOperators.Exists : ValueFilterOperators.Eq;
			if (expr.NodeType == ExpressionType.NotEqual)
			{
				op = value == null ? ValueFilterOperators.Exists : ValueFilterOperators.Eq;
				negative = !negative;
			}
			else if (expr.NodeType == ExpressionType.LessThan)
				op = ValueFilterOperators.Lt;
			else if (expr.NodeType == ExpressionType.GreaterThan)
				op = ValueFilterOperators.Gt;
			else if (expr.NodeType == ExpressionType.LessThanOrEqual)
				op = ValueFilterOperators.Le;
			else if (expr.NodeType == ExpressionType.GreaterThanOrEqual)
				op = ValueFilterOperators.Ge;
			if (op == null)
				throw new UnexpectedBinaryExpressionNodeTypeException();

			var dateValue = value as DateTime?;
			if (dateValue != null)
				value = dateValue.Value.ToUniversalTime().ToString("u");

			ProcessExpression(current, expr.Left, op, value, negative);
		}

		protected IModelFilterNode UnwindMemberExpression(IModelFilterNode current, MemberExpression expr)
		{
			if (expr.Expression == null || expr.Expression is ParameterExpression)
				return current;

			var membersStack = new Stack<MemberExpression>();
			var currentMemberExpr = expr.Expression as MemberExpression;
			if (currentMemberExpr == null)
				throw new UnexpectedExpressionTypeException(expr.Expression.Type);

			while (true)
			{
				membersStack.Push(currentMemberExpr);

				if (currentMemberExpr.Expression == null || currentMemberExpr.Expression is ParameterExpression)
					break;

				var prevMemberExpr = currentMemberExpr.Expression as MemberExpression;
				if (prevMemberExpr == null)
					throw new UnexpectedExpressionTypeException(currentMemberExpr.Expression.Type);

				currentMemberExpr = prevMemberExpr;
			}

			while (membersStack.Count > 0)
			{
				var propertyInfo = membersStack.Pop().PropertyInfoFromExpression();
				if (!typeof(IIdentifiedModel).IsAssignableFrom(propertyInfo.PropertyType))
					throw new OnlyModelPropertiesCanBeInChainException();

				var existing = current.Items.FirstOrDefault(filterNode => propertyInfo.Name.Equals(filterNode.Path));
				var existingModelFilter = existing as IModelFilterNode;
				if (existing != null && existingModelFilter == null)
					throw new FilterAlreadyContainsNodeWithSameNameException(existing.Path);

				if (existingModelFilter != null)
				{
					current = existingModelFilter;
					continue;
				}

				var newNode = new ModelFilterNode { Operator = ModelFilterOperators.And, Path = propertyInfo.Name };
				current.AddItem(newNode);
				current = newNode;
			}

			return current;
		}

		protected void ProcessMemberExpression(IModelFilterNode current, MemberExpression expr, ValueFilterOperators? op, object value, bool negative)
		{
			current = UnwindMemberExpression(current, expr);
			ProcessValueMemberExpression(current, expr, op, value, negative);
		}

		protected void ProcessValueMemberExpression(IModelFilterNode current, MemberExpression expr, ValueFilterOperators? op, object value, bool negative)
		{
			var propertyInfo = expr.PropertyInfoFromExpression();

			var propertyType = propertyInfo.PropertyType;
			if (propertyType.IsNullable())
				propertyType = propertyType.GetGenericArguments()[0];
			if (typeof(bool).IsAssignableFrom(propertyType))
			{
				op = op ?? ValueFilterOperators.Eq;
				value = value ?? true;
			}

			current.AddItem(new ValueFilterNode
			{
				Operator = op,
				Path = propertyInfo.Name,
				Operand = value.ConvertSafe<string>(),
				Negative = negative
			});
		}

		#endregion
	}
}
