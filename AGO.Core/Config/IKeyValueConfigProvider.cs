namespace AGO.Core.Config
{
	public interface IKeyValueConfigProvider<in TConfigurable> : IConfigProvider<TConfigurable>
		where TConfigurable : IKeyValueConfigurable
	{
	}
}