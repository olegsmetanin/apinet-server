using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AGO.Core.Application;
using AGO.Core.Json;
using AGO.Core.Localization;
using AGO.Core.Model.Security;
using NUnit.Framework;

namespace AGO.Core.Tests
{
	[TestFixture]
	public class EnumLocalizationTests : AbstractPersistenceApplication
	{
		[TestFixtureSetUp]
		public void SetUp()
		{
			Initialize();
		}

		[Test]
		public void EnumLocalized()
		{
			var lc = IocContainer.GetInstance<ILocalizationService>();
			var en = new CultureInfo("en");
			var ru = new CultureInfo("ru");
			var srt = typeof (SystemRole);
			Assert.AreEqual("Administrator", lc.MessageForType(srt, SystemRole.Administrator, en));
			Assert.AreEqual("Member", lc.MessageForType(srt, SystemRole.Member, en));
			Assert.AreEqual("Администратор", lc.MessageForType(srt, SystemRole.Administrator, ru));
			Assert.AreEqual("Участник", lc.MessageForType(srt, SystemRole.Member, ru));
		}

		[Test]
		public void ModelMetadalaForEnumLocalized()
		{
			const string en_meta_regex =
				@"""SystemRole"": {\s*""DisplayName"": ""System role"",\s*""PropertyType"": ""enum"",\s* ""PossibleValues"": {\s*""Member"": ""Member"",\s*""Administrator"": ""Administrator""\s*}\s*}";

			const string ru_meta_regex =
				@"""SystemRole"": {\s*""DisplayName"": ""Системная роль"",\s*""PropertyType"": ""enum"",\s* ""PossibleValues"": {\s*""Member"": ""Участник"",\s*""Administrator"": ""Администратор""\s*}\s*}";

			var en = new CultureInfo("en");
			var ru = new CultureInfo("ru");
			var metadata = new [] { _SessionProvider.ModelMetadata(typeof(UserModel)) };
			var jsonService = IocContainer.GetInstance<IJsonService>();
			jsonService.TryInitialize();
			var stringBuilder = new StringBuilder();

			System.Threading.Thread.CurrentThread.CurrentUICulture = en;
			var outputWriter = jsonService.CreateWriter(new StringWriter(stringBuilder), true);
			jsonService.CreateSerializer().Serialize(outputWriter, metadata);

			Assert.IsTrue(Regex.IsMatch(stringBuilder.ToString(), en_meta_regex, RegexOptions.Multiline));

			stringBuilder.Clear();
			System.Threading.Thread.CurrentThread.CurrentUICulture = ru;
			outputWriter = jsonService.CreateWriter(new StringWriter(stringBuilder), true);
			jsonService.CreateSerializer().Serialize(outputWriter, metadata);

			Assert.IsTrue(Regex.IsMatch(stringBuilder.ToString(), ru_meta_regex, RegexOptions.Multiline));
		}
	}
}