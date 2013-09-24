namespace AGO.Core
{
	public interface IInitializable
	{
		void Initialize();
	}

	public static class InitializableExtensions
	{
		public static void TryInitialize(this object service)
		{
			var initializable = service as IInitializable;
			if (initializable != null)
				initializable.Initialize();
		}
	}
}
