using System.Collections.Generic;

namespace AGO.Core.Controllers
{
	public class DictionaryStateStorage : IStateStorage
	{
		private readonly IDictionary<string, object> _Storage = 
			new Dictionary<string, object>();
		public object this[string key]
		{
			get
			{
				return !_Storage.ContainsKey(key) ? null : _Storage[key];
			}
			set
			{
				_Storage[key] = value;
			}
		}

		public void Remove(string key)
		{
			_Storage.Remove(key);
		}

		public void RemoveAll()
		{
			_Storage.Clear();
		}
	}
}
