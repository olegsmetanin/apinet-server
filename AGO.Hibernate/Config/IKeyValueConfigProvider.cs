namespace AGO.Hibernate.Config
{
	public interface IKeyValueConfigProvider<in TConfigurable> : IConfigProvider<TConfigurable>
		where TConfigurable : IKeyValueConfigurable
	{
	}
}