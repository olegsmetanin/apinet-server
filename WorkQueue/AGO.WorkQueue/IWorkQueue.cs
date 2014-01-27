﻿using System;
using System.Collections.Generic;

namespace AGO.WorkQueue
{
	/// <summary>
	/// Очередь задач
	/// </summary>
	public interface IWorkQueue
	{
		/// <summary>
		/// Добавляет запись в очередь задач
		/// </summary>
		/// <param name="item">Запись для очереди задач</param>
		void Add(QueueItem item);

		/// <summary>
		/// Возвращает следующий наиболее приоритетный элемент из очереди задач
		/// и удаляет его из очереди
		/// </summary>
		/// <param name="project">Код проекта, для которого выбирается очередная задача на выполение</param>
		/// <returns>Ссылка на задачу для выполнения или null, если для заданного проекта задач нет</returns>
		QueueItem Get(string project);

		/// <summary>
		/// Возвращает список уникальных кодов проектов из задач, находящихся в очереди
		/// </summary>
		/// <returns>Уникальный список кодов проектов</returns>
		IEnumerable<string> UniqueProjects(); 
		
		/// <summary>
		/// Возвращает состояние очереди в сыром виде
		/// </summary>
		/// <returns>Очередь задач (в сыром виде, без сортировок и приоритетов)</returns>
		IEnumerable<QueueItem> Dump();

		/// <summary>
		/// Возвращает словарь вида UserId -> User work items с сортировкой по приоритету согласно алгоритма приоритизации
		/// </summary>
		/// <returns>Словарь задач в очереди с разбиением по пользователям и сортировкой по приоритету</returns>
		IDictionary<Guid, IDictionary<string, QueueItem[]>> Snapshot();

		/// <summary>
		/// Очищает очередь задач
		/// </summary>
		void Clear();
	}
}