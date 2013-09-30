using System;
using AGO.Core;
using AGO.Core.Model.Dictionary;
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

			PopulateParamTypes("Docs1", admin);
			PopulateParamTypes("Docs2", admin);

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
						           		Priority = priority,
						           		Content = content,
						           		DueDate = dueDate
						           	};
						task.ChangeStatus(status, admin);
						task.TaskType = CurrentSession.QueryOver<TaskTypeModel>()
							.Where(m => m.ProjectCode == project && m.Name == type).SingleOrDefault();

						seqnum++;
						return task;
					};

			var t1 = factory("Инвентаризация", TaskStatus.NotStarted, TaskPriority.Normal, null, null);
			var t2 = factory("Расчет по схеме", TaskStatus.InWork, TaskPriority.High, "Расчет по схеме 2",
			                 DateTime.Now.AddDays(2));
			var t3 = factory("Обмер на объекте", TaskStatus.Completed, TaskPriority.Low, "Выполнить обмеры на объекте по адресу МО, Королев, Космонавтов 12, вл. 2",
							 DateTime.Now.AddDays(3));
			var t4 = factory("Инвентаризация", TaskStatus.NotStarted, TaskPriority.High, null, DateTime.Now.AddDays(-1));

			CurrentSession.Save(t1);
			CurrentSession.Save(t2);
			CurrentSession.Save(t3);
			CurrentSession.Save(t4);

			Func<TaskModel, string, object, TaskCustomPropertyModel> paramFactory =
				(task, name, value) => new TaskCustomPropertyModel
				    {
				        Task = task,
				        Creator = admin,
				        PropertyType = CurrentSession.QueryOver<CustomPropertyTypeModel>()
				            .Where(m => m.ProjectCode == project && m.FullName == name).SingleOrDefault(),
				        Value = value
				    };

			var sp1 = paramFactory(t1, "str", "some string data");
			var np1 = paramFactory(t1, "num", 12.3);
			var dp1 = paramFactory(t1, "date", new DateTime(2013, 01, 01));

			CurrentSession.Save(sp1);
			CurrentSession.Save(np1);
			CurrentSession.Save(dp1);
		}

		private void PopulateParamTypes(string project, UserModel admin)
		{
			Func<string, CustomPropertyValueType, CustomPropertyTypeModel> factory =
				(name, type) => new CustomPropertyTypeModel {ProjectCode = project, Creator = admin, Name = name,  FullName = name, ValueType = type};

			var strParam = factory("str", CustomPropertyValueType.String);
			var numParam = factory("num", CustomPropertyValueType.Number);
			var dateParam = factory("date", CustomPropertyValueType.Date);

			CurrentSession.Save(strParam);
			CurrentSession.Save(numParam);
			CurrentSession.Save(dateParam);
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