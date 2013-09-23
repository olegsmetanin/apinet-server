using System;
using AGO.Core;
using AGO.Core.Model.Security;
using AGO.Tasks.Model.Dictionary;
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

			//TODO other models
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
	}
}