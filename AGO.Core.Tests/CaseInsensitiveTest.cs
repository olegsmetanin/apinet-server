using System.Linq;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using NHibernate.Criterion;
using NUnit.Framework;

namespace AGO.Core.Tests
{
	public class CaseInsensitiveTest: AbstractPersistenceTest<ModelHelper>
	{
		protected override void CreateModelHelpers()
		{
			FM = new ModelHelper(() => MainSession, () => CurrentUser);
			M = new ModelHelper(() => MainSession, () => CurrentUser);
		}

		private void AssertFoundAndEqual(ProjectModel lowerCaseProj, ProjectModel upperCaseProj)
		{
			Assert.That(lowerCaseProj, Is.Not.Null);
			Assert.That(upperCaseProj, Is.Not.Null);
			Assert.That(lowerCaseProj, Is.EqualTo(upperCaseProj));
		}

		[Test]
		public void EqualInQueryOverCaseInsensitive()
		{
			var lCrmProj = MainSession.QueryOver<ProjectModel>().Where(m => m.ProjectCode == "crm").SingleOrDefault();
			var uCrmProj = MainSession.QueryOver<ProjectModel>().Where(m => m.ProjectCode == "CRM").SingleOrDefault();

			AssertFoundAndEqual(lCrmProj, uCrmProj);
		}

		[Test]
		public void LikeInQueryOverCaseInsensitive()
		{
			var lCrmProj = MainSession.QueryOver<ProjectModel>()
				.WhereRestrictionOn(m => m.ProjectCode)
				.IsLike("cr", MatchMode.Anywhere)
				.SingleOrDefault();
			var uCrmProj = MainSession.QueryOver<ProjectModel>()
				.WhereRestrictionOn(m => m.ProjectCode)
				.IsLike("CR", MatchMode.Anywhere)
				.SingleOrDefault();

			AssertFoundAndEqual(lCrmProj, uCrmProj);
		}

		[Test]
		public void InsensitiveLikeInQueryOverCaseInsensitive()
		{
			var lCrmProj = MainSession.QueryOver<ProjectModel>()
				.WhereRestrictionOn(m => m.ProjectCode)
				.IsInsensitiveLike("cr", MatchMode.Anywhere)
				.SingleOrDefault();
			var uCrmProj = MainSession.QueryOver<ProjectModel>()
				.WhereRestrictionOn(m => m.ProjectCode)
				.IsInsensitiveLike("CR", MatchMode.Anywhere)
				.SingleOrDefault();

			AssertFoundAndEqual(lCrmProj, uCrmProj);
		}

		[Test]
		public void EqualInFilteringServiceCaseInsensitive()
		{
			var fb = _FilteringService.Filter<ProjectModel>();
			var dao = DaoFactory.CreateMainFilteringDao();
			IModelFilterNode lPredicate = fb.Where(m => m.ProjectCode == "crm");
			IModelFilterNode uPredicate = fb.Where(m => m.ProjectCode == "CRM");
			var lCrmProj = dao.Find<ProjectModel>(lPredicate);
			var uCrmProj = dao.Find<ProjectModel>(uPredicate);

			AssertFoundAndEqual(lCrmProj, uCrmProj);
		}

		[Test]
		public void LikeInFilteringServiceCaseInsensitive()
		{
			var fb = _FilteringService.Filter<ProjectModel>();
			var dao = DaoFactory.CreateMainFilteringDao();
			IModelFilterNode lPredicate = fb.WhereString(m => m.ProjectCode).Like("cr", true, true);
			IModelFilterNode uPredicate = fb.WhereString(m => m.ProjectCode).Like("CR", true, true);
			var lCrmProj = dao.Find<ProjectModel>(lPredicate);
			var uCrmProj = dao.Find<ProjectModel>(uPredicate);

			AssertFoundAndEqual(lCrmProj, uCrmProj);
		}

		[Test]
		public void EqualInCriteriaCaseInsensitive()
		{
			var lCriteria = DetachedCriteria.For<ProjectModel>();
			lCriteria.Add(Restrictions.Eq(Projections.Property<ProjectModel>(m => m.ProjectCode), "crm"));

			var uCriteria = DetachedCriteria.For<ProjectModel>();
			uCriteria.Add(Restrictions.Eq(Projections.Property<ProjectModel>(m => m.ProjectCode), "CRM"));

			var lCrmProj = lCriteria.GetExecutableCriteria(MainSession).List<ProjectModel>().FirstOrDefault();
			var uCrmProj = uCriteria.GetExecutableCriteria(MainSession).List<ProjectModel>().FirstOrDefault();

			AssertFoundAndEqual(lCrmProj, uCrmProj);
		}

		[Test]
		public void LikeInCriteriaCaseInsensitive()
		{
			var lCriteria = DetachedCriteria.For<ProjectModel>();
			lCriteria.Add(Restrictions.Like(Projections.Property<ProjectModel>(m => m.ProjectCode), "cr", MatchMode.Anywhere));

			var uCriteria = DetachedCriteria.For<ProjectModel>();
			uCriteria.Add(Restrictions.Like(Projections.Property<ProjectModel>(m => m.ProjectCode), "CR", MatchMode.Anywhere));

			var lCrmProj = lCriteria.GetExecutableCriteria(MainSession).List<ProjectModel>().FirstOrDefault();
			var uCrmProj = uCriteria.GetExecutableCriteria(MainSession).List<ProjectModel>().FirstOrDefault();

			AssertFoundAndEqual(lCrmProj, uCrmProj);
		}

		[Test]
		public void InsensitiveLikeInCriteriaCaseInsensitive()
		{
			var lCriteria = DetachedCriteria.For<ProjectModel>();
			lCriteria.Add(Restrictions.InsensitiveLike(Projections.Property<ProjectModel>(m => m.ProjectCode), "cr", MatchMode.Anywhere));

			var uCriteria = DetachedCriteria.For<ProjectModel>();
			uCriteria.Add(Restrictions.InsensitiveLike(Projections.Property<ProjectModel>(m => m.ProjectCode), "CR", MatchMode.Anywhere));

			var lCrmProj = lCriteria.GetExecutableCriteria(MainSession).List<ProjectModel>().FirstOrDefault();
			var uCrmProj = uCriteria.GetExecutableCriteria(MainSession).List<ProjectModel>().FirstOrDefault();

			AssertFoundAndEqual(lCrmProj, uCrmProj);
		}

		[Test]
		public void EqualInHqlCaseInsensitive()
		{
			const string lQuery = "from ProjectModel pm where pm.ProjectCode = 'crm'";
			const string uQuery = "from ProjectModel pm where pm.ProjectCode = 'CRM'";

			var lCrmProj = MainSession.CreateQuery(lQuery).List<ProjectModel>().FirstOrDefault();
			var uCrmProj = MainSession.CreateQuery(uQuery).List<ProjectModel>().FirstOrDefault();

			AssertFoundAndEqual(lCrmProj, uCrmProj);

			const string parametrized = "from ProjectModel pm where pm.ProjectCode = :code";

			lCrmProj = MainSession.CreateQuery(parametrized).SetParameter("code", "crm").List<ProjectModel>().FirstOrDefault();
			uCrmProj = MainSession.CreateQuery(parametrized).SetParameter("code", "CRM").List<ProjectModel>().FirstOrDefault();

			AssertFoundAndEqual(lCrmProj, uCrmProj);
		}

		[Test]
		public void LikeInHqlCaseInsensitive()
		{
			const string lQuery = "from ProjectModel pm where pm.ProjectCode like '%cr%'";
			const string uQuery = "from ProjectModel pm where pm.ProjectCode like '%CR%'";

			var lCrmProj = MainSession.CreateQuery(lQuery).List<ProjectModel>().FirstOrDefault();
			var uCrmProj = MainSession.CreateQuery(uQuery).List<ProjectModel>().FirstOrDefault();

			AssertFoundAndEqual(lCrmProj, uCrmProj);

			const string parametrized = "from ProjectModel pm where pm.ProjectCode like :code";

			lCrmProj = MainSession.CreateQuery(parametrized).SetParameter("code", "%cr%").List<ProjectModel>().FirstOrDefault();
			uCrmProj = MainSession.CreateQuery(parametrized).SetParameter("code", "%CR%").List<ProjectModel>().FirstOrDefault();

			AssertFoundAndEqual(lCrmProj, uCrmProj);
		}
	}
}