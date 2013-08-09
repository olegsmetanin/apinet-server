using NHibernate;

namespace AGO.Hibernate
{
	public interface ISessionProvider
	{
		ISession CurrentSession { get; }

		void CloseCurrentSession(bool forceRollback = false);

		ISessionFactory SessionFactory { get; }
	}
}
