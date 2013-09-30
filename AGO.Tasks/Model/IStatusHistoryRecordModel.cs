﻿using System;
using AGO.Core.Model.Security;

namespace AGO.Tasks.Model
{
	/// <summary>
	/// Интерфейс модели записи в истории изменения статуса
	/// </summary>
	/// <typeparam name="TModel">Тип объекта-владельца статуса и истории</typeparam>
	/// <typeparam name="TStatus">Тип статуса</typeparam>
	/// TODO should be in core
	public interface IStatusHistoryRecordModel<TModel, TStatus>: ISecureModel
	{
		TModel Holder { get; set; }

		TStatus Status { get; set; }

		DateTime Start { get; set; }

		DateTime? Finish { get; set; }

		bool IsOpen { get; }
	}
}