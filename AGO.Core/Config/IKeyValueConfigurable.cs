namespace AGO.Core.Config
{
	public interface IKeyValueConfigurable : IConfigurable
	{
		string GetConfigProperty(string key);

		void SetConfigProperty(string key, string value);
	}
}