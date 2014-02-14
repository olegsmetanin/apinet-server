using System;
using System.Linq;
using AGO.Core.Application;
using AGO.Core.Filters;
using AGO.Core.Model;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using NHibernate;
using NUnit.Framework;

namespace AGO.Core.Tests.Security
{
	[TestFixture]
	public class SecurityServiceTests: AbstractPersistenceApplication
	{
		private string project;
		private UserModel admin;

		[TestFixtureSetUp]
		public void TestSetUp()
		{
			Initialize();
			project = "crm"; //from core test data service
		}

		private ISecurityService ss;
		private ISessionProvider sp;
		private IFilteringService fs;

		[SetUp]
		public void SetUp()
		{
			sp = IocContainer.GetInstance<ISessionProvider>();
			fs = IocContainer.GetInstance<IFilteringService>();
			ss = new SecurityService(fs);
			admin = sp.CurrentSession.QueryOver<UserModel>()
				.Where(m => m.Login == "admin@apinet-test.com").SingleOrDefault();
		}

		[TearDown]
		public void TearDown()
		{
			sp.CloseCurrentSession();
			ss = null;
			admin = null;
		}

		[Test]
		public void CreateServiceWithoutDependencyThrow()
		{
			Assert.That(() => new SecurityService(null), 
				Throws.Exception.TypeOf<ArgumentNullException>()
				.With.Property("ParamName").EqualTo("filteringService"));
		}

		[Test]
		public void CannotRegisterNullProvider()
		{
			Assert.That(() => ss.RegisterProvider(null), 
				Throws.Exception.TypeOf<ArgumentNullException>()
				.With.Property("ParamName").EqualTo("provider"));
		}

		[Test]
		public void ServiceReturnEmptyRestrictionsWithoutProviders()
		{
			var filter = ss.ApplyReadConstraint<ProjectModel>(null, admin.Id, sp.CurrentSession);

			Assert.That(filter, Is.Not.Null);
			Assert.That(filter.Items, Is.Empty);
		}

		[Test]
		public void ServiceApplyRestrictionFromRegisteredProvider()
		{
			//arrange
			ss.RegisterProvider(new ByTagsProjectSecurityProvider(fs, "Urgent"));
			//act
			var filter = ss.ApplyReadConstraint<ProjectModel>(project, admin.Id, sp.CurrentSession);
			//assert
			Assert.That(filter, Is.Not.Null);
			Assert.That(filter.Items.Count(), Is.EqualTo(1));
			var tagsPath = filter.Items.First() as IModelFilterNode;
			Assert.That(tagsPath, Is.Not.Null);
			Assert.That(tagsPath.Path, Is.EqualTo("Tags"));
			Assert.That(tagsPath.Items.Count(), Is.EqualTo(1));
			var tagPath = tagsPath.Items.First() as IModelFilterNode;
			Assert.That(tagPath, Is.Not.Null);
			Assert.That(tagPath.Path, Is.EqualTo("Tag"));
			Assert.That(tagPath.Items.Count(), Is.EqualTo(1));
			var fullNameRestriction = tagPath.Items.First() as IValueFilterNode;
			Assert.That(fullNameRestriction, Is.Not.Null);
			Assert.That(fullNameRestriction.Path, Is.EqualTo("FullName"));
			Assert.That(fullNameRestriction.Operand, Is.EqualTo("%Urgent%"));
			Assert.That(fullNameRestriction.Operator, Is.EqualTo(ValueFilterOperators.Like));
		}

		[Test]
		public void ServiceConcatcriteriaWithRestrictionFromRegisteredProvider()
		{
			//arrange
			ss.RegisterProvider(new ByTagsProjectSecurityProvider(fs, "Urgent"));
			var criteria = fs.Filter<ProjectModel>().Where(m => m.ProjectCode == project);
			//act
			var filter = ss.ApplyReadConstraint<ProjectModel>(project, admin.Id, sp.CurrentSession, criteria);
			//assert
			Assert.That(filter, Is.Not.Null);
			Assert.That(filter.Items.Count(), Is.EqualTo(2));

			Assert.That(filter.Items, Has.All.Matches(Is.InstanceOf<IModelFilterNode>()));

			var innerFilters = filter.Items.Cast<IModelFilterNode>();
			//Strange, but after where criteria placed in inner items collection
			Assert.That(innerFilters, Has.Exactly(1).Matches<IModelFilterNode>(f => f.Path == "Tags"));
			Assert.That(innerFilters, Has.Exactly(1).Matches<IModelFilterNode>(
				f => f.Items.First().Path == "ProjectCode"));
		}

//		[Test]
//		public void CombineSimpleWithDeepPath()
//		{
//			//app
//			var q = sp.CurrentSession.QueryOver<ProjectModel>()
//				.Where(m => m.CreationTime >= DateTime.Now.AddDays(-5));
//			//security provider
//			var criteria = AddConstraintsWithAliasCheck(q.UnderlyingCriteria);
//
//			Assert.That(() => criteria.List<ProjectModel>(), Is.Not.Null);
//		}
//
//		[Test]
//		public void CombineDeepPathWithSameDeepPath()
//		{
//			//app
//			var q = sp.CurrentSession.QueryOver<ProjectModel>()
//				.Where(m => m.CreationTime >= DateTime.Now.AddDays(-5))
//				.JoinQueryOver<ProjectToTagModel>(m => m.Tags)
//				.JoinQueryOver(m => m.Tag)
//				.Where(t => t.Name != "root");
//			//security provider
//			var criteria = AddConstraintsWithAliasCheck(q.UnderlyingCriteria);
//			//Так делать нельзя - дублируются джойны
//			//var criteria = AddConstraintsWithoutAliasCheck(q.UnderlyingCriteria); 
//			Assert.That(() => criteria.List<ProjectModel>(), Is.Not.Null);
//		}
//
//		[Test]
//		public void CombineSimpleWithDeepPathAsSubqueryExists()
//		{
//			//app
//			var q = sp.CurrentSession.QueryOver<ProjectModel>()
//				.Where(m => m.CreationTime >= DateTime.Now.AddDays(-5));
//			//security provider
//			var criteria = AddConstraintsAsExists(q.UnderlyingCriteria);
//
//			Assert.That(() => criteria.List<ProjectModel>(), Is.Not.Null);
//		}
//
//		private ICriteria AddConstraintsWithAliasCheck(ICriteria criteria)
//		{
//			var byTagName = Restrictions.Where<TagModel>(t => t.Name == "Test");
//			var byTagFullName = Restrictions.On<TagModel>(t => t.FullName).IsLike("/test", MatchMode.Start);
//			var byTag = Restrictions.Or(byTagName, byTagFullName);
//
//			var p2tCriteria = criteria.GetCriteriaByPath("Tags") ?? criteria.CreateCriteria("Tags");
//			var tagCriteria = p2tCriteria.GetCriteriaByPath("Tag") ?? p2tCriteria.CreateCriteria("Tag");
//			tagCriteria.Add(byTag);
//
//			return criteria;
//		}
//
//		private ICriteria AddConstraintsAsExists(ICriteria criteria)
//		{
//			var byTagName = Restrictions.Where<TagModel>(t => t.Name == "Test");
//			var byTagFullName = Restrictions.On<TagModel>(t => t.FullName).IsLike("/test", MatchMode.Start);
//			var byTag = Restrictions.Or(byTagName, byTagFullName);
//
//			var subquery = DetachedCriteria.For<ProjectToTagModel>("p_to_t");//must name, because same as alias in root criteria
//			subquery.CreateCriteria("Tag", "t").Add(byTag);
//			var innerProjId = Projections.Property<ProjectToTagModel>(m => m.ProjectId);
//			var pmAlias = criteria.Alias;
//			var outerProjId = Property.ForName(
//				pmAlias + "." + Projections.Property<ProjectModel>(m => m.Id).PropertyName);
//				//Projections.Alias(Projections.Property<ProjectModel>(m => m.Id), pmAlias); not worked
//				//Property.ForName(pmAlias + ".Id"); worked, but not typesafe
//				//Projections.Property<ProjectModel>(m => m.Id); not worked, and withAlias too
//			var correlation = Restrictions.EqProperty(innerProjId, outerProjId);
//			subquery.Add(correlation);
//			subquery.SetProjection(Projections.Constant(1));
//
//			criteria.Add(Subqueries.Exists(subquery));
//
//			return criteria;
//		}
//
//		[Test]
//		public void CombineSimpleFilterWithDeepPathFilter()
//		{
//			//app
//			var fs = IocContainer.GetInstance<IFilteringService>();
//			var appFilter = fs.Filter<ProjectModel>().Where(m => m.CreationTime >= DateTime.Now.AddDays(-5));
//			//security provider
//			var securityFilter = GetConstraints();
//			var filter = fs.ConcatFilters(new[] {appFilter, securityFilter});
//			var criteria = fs.CompileFilter(filter, typeof (ProjectModel))
//				.GetExecutableCriteria(sp.CurrentSession);
//
//			Assert.That(() => criteria.List<ProjectModel>(), Is.Not.Null);
//		}
//
//		private IModelFilterNode GetConstraints()
//		{
//			var f = IocContainer.GetInstance<IFilteringService>().Filter<ProjectModel>();
//			return f.WhereCollection(m => m.Tags).WhereString(m => m.Tag.FullName).Like("/test", appendWildcard: true);
//			//this is not work
////					.WhereModel(m => m.Tag)
////					.WhereString(m => m.FullName).Like("/test", appendWildcard: true);
//		}

		//Так нельзя, дублируются джойны
//		private ICriteria AddConstraintsWithoutCheck(ICriteria criteria)
//		{
//			var byTagName = Restrictions.Where<TagModel>(t => t.Name == "Test");
//			var byTagFullName = Restrictions.On<TagModel>(t => t.FullName).IsLike("/test", MatchMode.Start);
//			var byTag = Restrictions.Or(byTagName, byTagFullName);
//
//			criteria.CreateCriteria("Tags").CreateCriteria("Tag").Add(byTag);
//
//			return criteria;
//		}

		private class ByTagsProjectSecurityProvider : ISecurityConstraintsProvider
		{
			public string RequiredTag;
			private IFilteringService fs;

			public ByTagsProjectSecurityProvider(IFilteringService fs, string tag = null)
			{
				this.fs = fs;
				RequiredTag = tag;
			}

			public bool AcceptRead(Type modelType)
			{
				return typeof (ProjectModel).IsAssignableFrom(modelType);
			}

			public bool AcceptChange(IIdentifiedModel model)
			{
				return model is ProjectModel;
			}

			public IModelFilterNode ReadConstraint(string project, Guid userId, ISession session)
			{
				var builder = fs.Filter<ProjectModel>();
				return builder
					.WhereCollection(m => m.Tags)
					.WhereString(m => m.Tag.FullName)
					.Like(RequiredTag, true, true);
			}
		}
	}
}