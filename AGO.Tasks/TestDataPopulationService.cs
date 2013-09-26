using System;
using AGO.Core;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Dictionary;
using AGO.Tasks.Model.Task;
using NHibernate;

namespace AGO.Tasks
{
	public class TestDataPopulationService : AbstractService, ITestDataPopulationService
	{
		private readonly ISessionProvider _SessionProvider;

		private ISession CurrentSession { get { return _SessionProvider.CurrentSession; } }

		public TestDataPopulationService(ISessionProvider sessionProvider)
		{
			if (sessionProvider == null)
				throw new ArgumentNullException("sessionProvider");
			_SessionProvider = sessionProvider;
		}

		public void Populate()
		{
			var admin = CurrentSession.QueryOver<UserModel>()
				.Where(m => m.SystemRole == SystemRole.Administrator).SingleOrDefault();
			if (admin == null)
				throw new Exception("admin is null");

			PopulateTaskTypes("Docs1", admin);
			PopulateTaskTypes("Docs2", admin);

			PopulateCustomStatuses("Docs1", admin);
			PopulateCustomStatuses("Docs2", admin);

			_SessionProvider.CloseCurrentSession();

			PopulateTasks("Docs1", admin);
			PopulateTasks("Docs2", admin);
		}

		private void PopulateTasks(string project, UserModel admin)
		{
			var seqnum = 1;
			Func<string, TaskStatus, TaskPriority, string, DateTime?, TaskModel> factory =
				(type, status, priority, content, dueDate) =>
					{
						var task = new TaskModel
						           	{
						           		Creator = admin,
						           		ProjectCode = project,
						           		InternalSeqNumber = seqnum,
						           		SeqNumber = "t0-" + seqnum,
						           		Status = status,
						           		Priority = priority,
						           		Content = content,
						           		DueDate = dueDate
						           	};
						task.TaskType = CurrentSession.QueryOver<TaskTypeModel>()
							.Where(m => m.ProjectCode == project && m.Name == type).SingleOrDefault();

						seqnum++;
						return task;
					};

			var t1 = factory("Инвентаризация", TaskStatus.NotStarted, TaskPriority.Normal, null, null);
			var t2 = factory("Расчет по схеме", TaskStatus.InWork, TaskPriority.High, "Расчет по схеме 2",
			                 DateTime.Now.AddDays(2));

			CurrentSession.Save(t1);
			CurrentSession.Save(t2);
		}

		private void PopulateTaskTypes(string project, UserModel admin)
		{
			Func<string, TaskTypeModel> factory = 
				name => new TaskTypeModel {Creator = admin, ProjectCode = project, Name = name};

			var invent = factory("Инвентаризация");
			var workout = factory("Обмер на объекте");
			var calc = factory("Расчет по схеме");
			var payment = factory("Подготовка документов на оплату");
			var clean = factory("Очистка архивов");
			var prep = factory("Подготовка места работы");

			CurrentSession.Save(invent);
			CurrentSession.Save(workout);
			CurrentSession.Save(calc);
			CurrentSession.Save(payment);
			CurrentSession.Save(clean);
			CurrentSession.Save(prep);
		}

		private void PopulateCustomStatuses(string project, UserModel admin)
		{
			byte order = 0;
			Func<string, CustomTaskStatusModel> factory =
				name => new CustomTaskStatusModel {Creator = admin, ProjectCode = project, Name = name, ViewOrder = order++};

			var prep = factory("Подготовка");
			var towork = factory("Передано в работу");
			var progress = factory("Исполнение");
			var complete = factory("Выполнено");
			var closed = factory("Закрыто");
			var susp = factory("Приостановлено");

			CurrentSession.Save(prep);
			CurrentSession.Save(towork);
			CurrentSession.Save(progress);
			CurrentSession.Save(complete);
			CurrentSession.Save(closed);
			CurrentSession.Save(susp);
		}
	}
}