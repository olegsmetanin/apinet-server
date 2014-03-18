using System;
using System.Linq;
using AGO.Core.Filters;
using AGO.Core.Model.Projects;
using AGO.Core.Model.Security;
using AGO.Core.Security;
using NSubstitute;
using NUnit.Framework;

namespace AGO.Core.Tests.Security
{
	public class SecurityServiceTests: AbstractPersistenceTest<ModelHelper>
	{
		private ProjectModel testProject;
		private UserModel admin;

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			testProject = FM.Project(Guid.NewGuid().ToString().Replace("-", string.Empty));
			admin = LoginAdmin();
		}

		private ISecurityService ss;
		private IFilteringService fs;

		public override void SetUp()
		{
			base.SetUp();

			fs = IocContainer.GetInstance<IFilteringService>();
			ss = new SecurityService(fs);			
		}

		public override void TearDown()
		{
			ss = null;
		}

		protected override void CreateModelHelpers()
		{
			FM = new ModelHelper(() => MainSession, () => CurrentUser);
			//M = ... not used
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
			var filter = ss.ApplyReadConstraint<ProjectModel>(null, admin.Id, MainSession);

			Assert.That(filter, Is.Not.Null);
			Assert.That(filter.Items, Is.Empty);
		}

		[Test]
		public void ServiceApplyRestrictionFromRegisteredProvider()
		{
			var testFilter = new ModelFilterNode {Path = "testpath"};
			//arrange
			var mock = Substitute.For<ISecurityConstraintsProvider>();
			mock.AcceptRead(null, null, null).ReturnsForAnyArgs(true);
			mock.ReadConstraint(null, Guid.Empty, null).ReturnsForAnyArgs(testFilter);
			ss.RegisterProvider(mock);
			//act
			var filter = ss.ApplyReadConstraint<ProjectModel>(testProject.ProjectCode, admin.Id, MainSession);
			//assert
			Assert.That(filter, Is.Not.Null);
			Assert.That(filter.Items.Count(), Is.EqualTo(1));
			var tagsPath = filter.Items.First() as IModelFilterNode;
			Assert.That(tagsPath, Is.Not.Null);
// ReSharper disable once PossibleNullReferenceException
			Assert.That(tagsPath.Path, Is.EqualTo(testFilter.Path));
		}

		[Test]
		public void ServiceConcatCriteriaWithRestrictionFromRegisteredProvider()
		{
			var testFilter = new ModelFilterNode { Path = "testpath" };
			//arrange
			var mock = Substitute.For<ISecurityConstraintsProvider>();
			mock.AcceptRead(null, null, null).ReturnsForAnyArgs(true);
			mock.ReadConstraint(null, Guid.Empty, null).ReturnsForAnyArgs(testFilter);
			ss.RegisterProvider(mock);
			var criteria = fs.Filter<ProjectModel>().Where(m => m.ProjectCode == testProject.ProjectCode);
			//act
			var filter = ss.ApplyReadConstraint<ProjectModel>(testProject.ProjectCode, admin.Id, MainSession, criteria);
			//assert
			Assert.That(filter, Is.Not.Null);
			Assert.That(filter.Items.Count(), Is.EqualTo(2));

			Assert.That(filter.Items, Has.All.Matches(Is.InstanceOf<IModelFilterNode>()));

			var innerFilters = filter.Items.Cast<IModelFilterNode>();
			//Strange, but after where criteria placed in inner items collection
			Assert.That(innerFilters, Has.Exactly(1).Matches<IModelFilterNode>(f => f.Path == testFilter.Path));
			Assert.That(innerFilters, Has.Exactly(1).Matches<IModelFilterNode>(f => 
				f.Items.Count() == 1 && f.Items.First().Path == "ProjectCode"));
		}

		[Test]
		public void ServiceConcatCriteriaOnlyFromAcceptableProviders()
		{
			//arrange
			var filter1 = new ModelFilterNode {Path = "f1"};
			var filter2 = new ModelFilterNode { Path = "f2" };
			var mock1 = Substitute.For<ISecurityConstraintsProvider>();
			var mock2 = Substitute.For<ISecurityConstraintsProvider>();
			mock1.AcceptRead(null, null, null).ReturnsForAnyArgs(true);
			mock2.AcceptRead(null, null, null).ReturnsForAnyArgs(false);
			mock1.ReadConstraint(null, Guid.Empty, null).ReturnsForAnyArgs(filter1);
			mock2.ReadConstraint(null, Guid.Empty, null).ReturnsForAnyArgs(filter2);
			ss.RegisterProvider(mock1);
			ss.RegisterProvider(mock2);
			var criteria = fs.Filter<ProjectModel>().Where(m => m.ProjectCode == testProject.ProjectCode);

			//act
			var filter = ss.ApplyReadConstraint<ProjectModel>(testProject.ProjectCode, admin.Id, MainSession, criteria);

			//assert
			Assert.That(filter, Is.Not.Null);
			Assert.That(filter.Items.Count(), Is.EqualTo(2));

			Assert.That(filter.Items, Has.All.Matches(Is.InstanceOf<IModelFilterNode>()));

			var innerFilters = filter.Items.Cast<IModelFilterNode>();
			Assert.That(innerFilters, Has.Exactly(1).Matches<IModelFilterNode>(f => f.Path == filter1.Path));
			Assert.That(innerFilters, Has.None.Matches<IModelFilterNode>(f => f.Path == filter2.Path));
			Assert.That(innerFilters, Has.Exactly(1).Matches<IModelFilterNode>(f =>
				f.Items.Count() == 1 && f.Items.First().Path == "ProjectCode"));
		}

		[Test]
		public void ServiceDemandDoesNotThrowWithoutProviders()
		{
			Assert.That(() => ss.DemandUpdate(testProject, testProject.ProjectCode, admin.Id, MainSession), Throws.Nothing);
			Assert.That(() => ss.DemandDelete(testProject, testProject.ProjectCode, admin.Id, MainSession), Throws.Nothing);
		}

		[Test]
		public void ServiceThrowIfOneOfProvidersDenyChange()
		{
			var denyMock = Substitute.For<ISecurityConstraintsProvider>();
			var grantMock = Substitute.For<ISecurityConstraintsProvider>();
			denyMock.AcceptChange(null, null, null).ReturnsForAnyArgs(true);
			grantMock.AcceptChange(null, null, null).ReturnsForAnyArgs(true);
			denyMock.CanCreate(null, null, Guid.Empty, null).ReturnsForAnyArgs(false);
			denyMock.CanUpdate(null, null, Guid.Empty, null).ReturnsForAnyArgs(false);
			denyMock.CanDelete(null, null, Guid.Empty, null).ReturnsForAnyArgs(false);
			grantMock.CanCreate(null, null, Guid.Empty, null).ReturnsForAnyArgs(true);
			grantMock.CanUpdate(null, null, Guid.Empty, null).ReturnsForAnyArgs(true);
			grantMock.CanDelete(null, null, Guid.Empty, null).ReturnsForAnyArgs(true);
			ss.RegisterProvider(denyMock);
			ss.RegisterProvider(grantMock);

			//new model
			var p = new ProjectModel {ProjectCode = "test"};
			Assert.That(() => ss.DemandUpdate(p, null, Guid.Empty, null), 
				Throws.Exception.TypeOf<CreationDeniedException>());

			//existing model
			Assert.That(() => ss.DemandUpdate(testProject, null, Guid.Empty, null),
				Throws.Exception.TypeOf<ChangeDeniedException>());
			Assert.That(() => ss.DemandDelete(p, null, Guid.Empty, null),
				Throws.Exception.TypeOf<DeleteDeniedException>());
		}

		#region First attempts for concat security and provided criterias. 
		//save this code as our query builder will not covered by tests and used everywhere

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
		#endregion
	}
}