using System;
using System.Collections.Generic;

namespace AGO.WorkQueue
{
	/// <summary>
	/// Базовый класс для реализаций очереди работ
	/// </summary>
	public abstract class AbstractWorkQueue: IWorkQueue
	{
		public abstract void Add(QueueItem item);
		public abstract QueueItem Get(string project);
		public abstract IEnumerable<string> UniqueProjects();
		public abstract IEnumerable<QueueItem> Dump();
		public abstract IDictionary<string, IDictionary<string, QueueItem[]>> Snapshot();
		public abstract void Clear();
	}
}