using System;

namespace AGO.Core.Json
{
	public class JsonRequestException : Exception
	{
		public JsonRequestException(Exception innerException)
			: base("Json request serialization/deserialization failure", innerException)
		{
		}
	}

	public class JsonRequestBodyEmptyException : Exception
	{
		public JsonRequestBodyEmptyException()
			: base("Empty request body")
		{
		}
	}

	public class JsonRequestBodyMissesPropertyException : Exception
	{
		public JsonRequestBodyMissesPropertyException(string property)
			: base(string.Format("No \"{0}\" property in request body", property))
		{
		}
	}
}
