namespace AGO.Hibernate.Config
{
	public interface IConfigProvider<in TConfigurable>
		where TConfigurable : IConfigurable
	{
		void ApplyTo(TConfigurable configurable);
	}
}