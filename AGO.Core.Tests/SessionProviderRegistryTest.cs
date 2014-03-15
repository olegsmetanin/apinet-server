using System;
using Npgsql;
using NUnit.Framework;

namespace AGO.Core.Tests
{
	public class SessionProviderRegistryTest : AbstractPersistenceTest<ModelHelper>
	{
		protected override void CreateModelHelpers()
		{
			FM = new ModelHelper(() => Session, () => CurrentUser);
			M = new ModelHelper(() => Session, () => CurrentUser);
		}

		public override void TearDown()
		{
			base.TearDown();

			SessionProviderRegistry.DropCachedProviders();
		}

		[Test]
		public void RegistryInitializedOnAppInitialize()
		{
			Assert.That(SessionProviderRegistry, Is.Not.Null);
		}

		[Test]
		public void RegistryHasWorkingMainDbConnection()
		{
			var main = SessionProviderRegistry.GetMainDbProvider();
			Assert.That(main, Is.Not.Null);

			var mainSession = main.CurrentSession;
			Assert.That(mainSession.IsConnected, Is.True);
		}

		[Test]
		public void RegistryHasWorkingProjectDbConnection()
		{
			var p1 = M.Project("p1");
			var p2 = M.Project("p2");
			var pgb = new NpgsqlConnectionStringBuilder(ConnectionString);
			pgb.ApplicationName = "p1";
			p1.ConnectionString = pgb.ConnectionString;
			pgb.ApplicationName = "p2";
			p2.ConnectionString = pgb.ConnectionString;
			Session.Update(p1);
			Session.Update(p2);
			Session.Flush();

			var p1Connection = SessionProviderRegistry.GetProjectProvider("p1");
			var p2Connection = SessionProviderRegistry.GetProjectProvider("p2");

			Assert.That(p1Connection.CurrentSession.IsConnected, Is.True);
			Assert.That(p1Connection.CurrentSession.Connection.ConnectionString, Contains.Substring("APPLICATIONNAME=p1"));
			Assert.That(p2Connection.CurrentSession.IsConnected, Is.True);
			Assert.That(p2Connection.CurrentSession.Connection.ConnectionString, Contains.Substring("APPLICATIONNAME=p2"));
		}

		[Test]
		public void RegistryCacheProjectDbConnection()
		{
			var p1 = M.Project("p1");
			var pgb = new NpgsqlConnectionStringBuilder(ConnectionString);
			pgb.ApplicationName = "p1";
			p1.ConnectionString = pgb.ConnectionString;
			Session.Update(p1);
			Session.Flush();

			var firstCallConnection = SessionProviderRegistry.GetProjectProvider("p1");
			var firstCallSession = firstCallConnection.CurrentSession;
			var secondCallConnection = SessionProviderRegistry.GetProjectProvider("p1");
			var secondCallSession = secondCallConnection.CurrentSession;

			Assert.That(firstCallConnection, Is.SameAs(secondCallConnection));
			Assert.That(firstCallSession, Is.SameAs(secondCallSession));
		}

		[Test]
		public void RegistryThrowOnInvalidProject()
		{
			Assert.That(() => SessionProviderRegistry.GetProjectProvider(null), 
				Throws.Exception.TypeOf<ArgumentNullException>());

			Assert.That(() => SessionProviderRegistry.GetProjectProvider(string.Empty),
				Throws.Exception.TypeOf<ArgumentNullException>());

			Assert.That(() => SessionProviderRegistry.GetProjectProvider(" "),
				Throws.Exception.TypeOf<ArgumentNullException>());

			Assert.That(() => SessionProviderRegistry.GetProjectProvider("asd"),
				Throws.Exception.TypeOf<NoSuchProjectException>());
		}

		[Test]
		public void RegistryCloseOnlyUsedSessions()
		{
			var p1 = M.Project("p1");
			var p2 = M.Project("p2");
			var p3 = M.Project("p3");
			p1.ConnectionString = ConnectionString;
			p2.ConnectionString = ConnectionString;
			p3.ConnectionString = ConnectionString;
			Session.Update(p1);
			Session.Update(p2);
			Session.Update(p3);
			Session.Flush();

			var mainSession = SessionProviderRegistry.GetMainDbProvider().CurrentSession;
			var p2Session = SessionProviderRegistry.GetProjectProvider("p2").CurrentSession;

			Assert.That(mainSession.IsConnected, Is.True);
			Assert.That(p2Session.IsConnected, Is.True);

			SessionProviderRegistry.CloseCurrentSessions();

			Assert.That(mainSession.IsConnected, Is.False);
			Assert.That(p2Session.IsConnected, Is.False);
		}

		private string ConnectionString
		{
			get { return KeyValueProvider.Value("Hibernate_connection.connection_string"); }
		}
	}
}