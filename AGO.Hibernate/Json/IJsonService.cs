using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace AGO.Hibernate.Json
{
	public interface IJsonService
	{
		JsonSerializer CreateSerializer();

		JsonReader CreateReader(
			TextReader reader,
			bool closeInput = false);

		JsonValidatingReader CreateValidatingReader(
			TextReader reader,
			JsonSchema schema,
			bool closeInput = false);

		JsonWriter CreateWriter(
			TextWriter writer,
			bool closeOutput = false);
	}
}