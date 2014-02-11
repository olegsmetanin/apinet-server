using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AGO.Core.Model.Security;

namespace AGO.Core.Model
{

	/// <summary>
	/// Вспомогательный класс для синхронного изменения статуса объекта и его истории статусов
	/// </summary>
	public static class StatusChangeHelper
	{
		private const string DEFAULT_STATUS_PROP = "Status";

		public static THistory Change<TModel, TStatus, THistory>(
			TModel holder,
			TStatus newStatus,
			ICollection<THistory> history,
			UserModel changer = null,
			Expression<Func<TModel, object>> holderStatusProp = null)
			where TModel : class
			where THistory : class, IStatusHistoryRecordModel<TModel, TStatus>, new()
		{
			if (holder == null)
				throw new ArgumentNullException("holder");
			if (typeof(TStatus).IsClass && ReferenceEquals(null, newStatus))
				throw new ArgumentNullException("newStatus");
			if (history == null)
				throw new ArgumentNullException("history");

			var statusPropName = holderStatusProp != null
			                     	? holderStatusProp.PropertyInfoFromExpression().Name
			                     	: DEFAULT_STATUS_PROP;

			var current = holder.GetMemberValue<TStatus>(statusPropName);
			if (!ChangeNeeded<TModel, TStatus, THistory>(current, newStatus, history))
				return null;

			//set new status
			holder.SetMemberValue(statusPropName, newStatus);
			//close current history record (record with finish == null)
			foreach (var record in history)
			{
				if (!record.IsOpen) continue;

				record.Finish = DateTime.Now;
				break; //only one record is current
			}
			//make new current history record
			var currentRecord = 
				new THistory {Holder = holder, Status = newStatus, Start = DateTime.UtcNow, Finish = null, Creator = changer};
			history.Add(currentRecord);

			return currentRecord;
		}

		private static bool ChangeNeeded<TModel, TStatus, THistory>(TStatus current, TStatus newStatus, IEnumerable<THistory> history)
			where THistory : class, IStatusHistoryRecordModel<TModel, TStatus>, new()
		{
			Func<TStatus, TStatus, bool> equals = (curr, next) => typeof(TStatus).IsValueType 
				? EqualityComparer<TStatus>.Default.Equals(current, newStatus)
				: newStatus.Equals(current); //Rely on the fact, that base IdentifiedModel override Equals and use Id for comparison
			
			if (!equals(current, newStatus)) return true;

			//Case, when statuses equals, but for current no record in history (when status is enum, that can happen)
			if (!history.Any(h => equals(h.Status, current))) return true;

			return false;
		}
	}
}