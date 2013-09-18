using System;
using Common.Logging;
using AGO.Core.Config;

namespace AGO.Core
{
	public abstract class AbstractService : IInitializable, IKeyValueConfigurable
	{
		public const string ModelName = "model";

		#region Configuration properties, fields and methods

		public string GetConfigProperty(string key)
		{
			key = key.TrimSafe();
			return DoGetConfigProperty(key);
		}

		public void SetConfigProperty(string key, string value)
		{
			if (_Ready)
				return;
			if (key.IsNullOrWhiteSpace())
				return;
			key = key.TrimSafe();
			DoSetConfigProperty(key, value);
		}

		#endregion

		#region Properties, fields, constructors

		protected ILog _Log;
		public ILog Log
		{
			get { _Log = _Log ?? LogManager.GetLogger(GetType()); return _Log; }
			set { _Log = value; }
		}

		protected bool _Ready;

		#endregion

		#region Interface methods

		public void Initialize()
		{
			try
			{
				if (_Ready)
					return;
				DoFinalizeConfig();
				DoInitialize();
				_Ready = true;
			}
			catch (Exception e)
			{
				Log.Fatal(e.Message, e);
				throw;
			}
		}

		#endregion

		#region Template methods

		protected virtual void DoFinalizeConfig()
		{
		}

		protected virtual void DoInitialize()
		{
		}

		protected virtual void DoSetConfigProperty(string key, string value)
		{
		}

		protected virtual string DoGetConfigProperty(string key)
		{
			return null;
		}

		#endregion
	}
}