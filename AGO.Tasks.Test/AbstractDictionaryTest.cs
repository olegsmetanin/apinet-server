using AGO.Tasks.Controllers;

namespace AGO.Tasks.Test
{
	public class AbstractDictionaryTest: AbstractTest
	{
		protected DictionaryController Controller { get; private set; }

		protected override void Init()
		{
			base.Init();
			Controller = IocContainer.GetInstance<DictionaryController>();
		}
	}
}