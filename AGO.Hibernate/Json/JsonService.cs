using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Schema;

namespace AGO.Hibernate.Json
{
	public class JsonService : AbstractService, IJsonService
	{
		#region Configuration properties, fields and methods

		private JsonSerializerSettings _Settings = new JsonSerializerSettings();
		public JsonSerializerSettings Settings
		{
			get { return _Settings; }
			set { _Settings = value ?? _Settings; }
		}

		private char _IndentChar;

		private int _Indentation;

		protected override void DoSetConfigProperty(string key, string value)
		{
			if ("IndentChar".Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				_IndentChar = value.ConvertSafe<char>();
				return;
			}
			if ("Indentation".Equals(key, StringComparison.InvariantCultureIgnoreCase))
			{
				_Indentation = value.ConvertSafe<int>();
				return;
			}

			_Settings.SetMemberValue(key, value);
		}

		protected override string DoGetConfigProperty(string key)
		{
			if ("IndentChar".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return _IndentChar.ToString(CultureInfo.InvariantCulture);
			if ("Indentation".Equals(key, StringComparison.InvariantCultureIgnoreCase))
				return _Indentation.ToString(CultureInfo.InvariantCulture);

			return _Settings.GetMemberValue(key).ToStringSafe();
		}

		#endregion

		#region Interfaces implementation

		public JsonSerializer CreateSerializer()
		{
			return JsonSerializer.Create(_Settings);
		}

		public JsonReader CreateReader(
			TextReader reader,
			bool closeInput)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			return new JsonTextReader(reader)
			{
				CloseInput = closeInput,
				DateTimeZoneHandling = _Settings.DateTimeZoneHandling,
				DateParseHandling = _Settings.DateParseHandling,
				Culture = _Settings.Culture,
				FloatParseHandling = _Settings.FloatParseHandling,
				MaxDepth = _Settings.MaxDepth
			};
		}

		public JsonValidatingReader CreateValidatingReader(
			TextReader reader,
			JsonSchema schema,
			bool closeInput)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");
			if (schema == null)
				throw new ArgumentNullException("schema");

			return new JsonValidatingReader(CreateReader(reader, closeInput))
			{
				CloseInput = true,
				Schema = schema
			};
		}

		public JsonWriter CreateWriter(
			TextWriter writer,
			bool closeOutput)
		{
			if (writer == null)
				throw new ArgumentNullException("writer");

			var jsonTextWriter = new JsonTextWriter(writer)
			{
				CloseOutput = closeOutput,
				Formatting = _Settings.Formatting,
				DateTimeZoneHandling = _Settings.DateTimeZoneHandling,
				DateFormatHandling = _Settings.DateFormatHandling,
				Culture = _Settings.Culture,
				DateFormatString = _Settings.DateFormatString,
				FloatFormatHandling = _Settings.FloatFormatHandling
			};

			if (_IndentChar != default(char))
				jsonTextWriter.IndentChar = _IndentChar;
			if (_Indentation != default(int))
				jsonTextWriter.Indentation = _Indentation;

			return jsonTextWriter;
		}

		#endregion

		#region Template methods

		protected override void DoFinalizeConfig()
		{
			base.DoFinalizeConfig();

			_Settings.Converters.Add(new StringEnumConverter());
			_Settings.ContractResolver = _Settings.ContractResolver ?? new ContractResolver();
		}

		#endregion
	}
}