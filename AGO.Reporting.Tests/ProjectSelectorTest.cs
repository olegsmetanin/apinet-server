using System;
using System.Collections.Generic;
using AGO.Reporting.Service;
using NUnit.Framework;

namespace AGO.Reporting.Tests
{
	[TestFixture]
	public class ProjectSelectorTest
	{
		private readonly Func<IEnumerable<string>> availableProjects = () => new[] {"a", "b", "c"};

		[Test]
		public void EmptyProjectThrow()
		{
			Assert.That(() => new ProjectSelector(null), 
				Throws.Exception
				.TypeOf<ArgumentNullException>()
				.With.Property("ParamName").EqualTo("projectMask"));

			Assert.That(() => new ProjectSelector(string.Empty),
				Throws.Exception
				.TypeOf<ArgumentNullException>()
				.With.Property("ParamName").EqualTo("projectMask"));

			Assert.That(() => new ProjectSelector("  "),
				Throws.Exception
				.TypeOf<ArgumentNullException>()
				.With.Property("ParamName").EqualTo("projectMask"));
		}

		[Test]
		public void SelectFromExternalWithoutExternalReturnEmpty()
		{
			var ps = new ProjectSelector("*");

			Assert.AreEqual(string.Empty, ps.NextProject(null));
			Assert.AreEqual(string.Empty, ps.NextProject(() => new String[0]));
		}

		[Test]
		public void SingleProjectAlwaysConstant()
		{
			const string proj = "proj";
			var ps = new ProjectSelector(proj);

			Assert.AreEqual(proj, ps.NextProject(null));
			Assert.AreEqual(proj, ps.NextProject(null));
			Assert.AreEqual(proj, ps.NextProject(availableProjects));
			Assert.AreEqual(proj, ps.NextProject(availableProjects));
		}

		[Test]
		public void SomeProjectsRotateRoundRobin()
		{
			var ps = new ProjectSelector("a,b,c");

			Assert.AreEqual("a", ps.NextProject(null));
			Assert.AreEqual("b", ps.NextProject(null));
			Assert.AreEqual("c", ps.NextProject(null));
			Assert.AreEqual("a", ps.NextProject(availableProjects));
			Assert.AreEqual("b", ps.NextProject(availableProjects));
			Assert.AreEqual("c", ps.NextProject(availableProjects));
		}

		[Test]
		public void SomeProjectsWithDuplicateRotateRoundRobin()
		{
			var ps = new ProjectSelector("a,a,b,c");

			Assert.AreEqual("a", ps.NextProject(null));
			Assert.AreEqual("a", ps.NextProject(null));
			Assert.AreEqual("b", ps.NextProject(null));
			Assert.AreEqual("c", ps.NextProject(null));
			Assert.AreEqual("a", ps.NextProject(availableProjects));
			Assert.AreEqual("a", ps.NextProject(availableProjects));
			Assert.AreEqual("b", ps.NextProject(availableProjects));
			Assert.AreEqual("c", ps.NextProject(availableProjects));
		}

		[Test]
		public void SelectFromExternalWithMemory()
		{
			var ps = new ProjectSelector("*");

			var proj = ps.NextProject(availableProjects);
			Assert.AreEqual("a", proj);
			var nproj = ps.NextProject(availableProjects);
			Assert.AreNotEqual(proj, nproj);
			proj = ps.NextProject(availableProjects);
			Assert.AreNotEqual(nproj, proj);

			ps = new ProjectSelector("*");
			Assert.AreEqual("a", ps.NextProject(availableProjects));
			Assert.AreEqual("c", ps.NextProject(() => new [] {"a", "c"}));
			Assert.AreEqual("c", ps.NextProject(() => new[] { "c" }));
		}
	}
}