namespace AGO.Core.Controllers
{
	public interface IStateStorage
	{
		object this[string key] { get; set; }

		void Remove(string key);

		void RemoveAll();
	}
}
