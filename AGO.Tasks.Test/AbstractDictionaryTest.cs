using AGO.Tasks.Controllers;

namespace AGO.Tasks.Test
{
	public class AbstractDictionaryTest: AbstractTest
	{
		protected DictionaryController Controller { get; private set; }

		public override void FixtureSetUp()
		{
			base.FixtureSetUp();
		
			Controller = IocContainer.GetInstance<DictionaryController>();
		}
	}
}