using System;
using System.Collections.Generic;

namespace AGO.Core.Controllers
{
	public class DictionaryStateStorage<TType> : IStateStorage<TType>
	{
		#region Properties, fields, constructors

		private readonly IDictionary<string, TType> _Storage =
			new Dictionary<string, TType>(); 

		#endregion

		#region Interfaces implementation

		public IEnumerable<string> Keys { get { return _Storage.Keys; } }

		public TType this[string key]
		{
			get
			{
				if (key.IsNullOrWhiteSpace())
					throw new ArgumentNullException("key");

				return !_Storage.ContainsKey(key) 
					? default(TType) 
					: _Storage[key];
			}
			set
			{
				if (key.IsNullOrWhiteSpace())
					throw new ArgumentNullException("key");

				_Storage[key] = value;
			}
		}

		public void Remove(string key)
		{
			if (key.IsNullOrWhiteSpace())
				throw new ArgumentNullException("key");

			_Storage.Remove(key);
		}

		public void RemoveAll()
		{
			_Storage.Clear();
		} 

		#endregion
	}
}
