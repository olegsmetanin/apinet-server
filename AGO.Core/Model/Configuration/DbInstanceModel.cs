using System;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Attributes.Model;
using Newtonsoft.Json;

namespace AGO.Core.Model.Configuration
{
	/// <summary>
	/// Address of database server instance
	/// </summary>
	/// <remarks>Used for provide avaliable db instances for user when project created</remarks>
	[RelationalModel]
	public class DbInstanceModel: IdentifiedModel<Guid>
	{
		/// <summary>
		/// Showed db server name
		/// </summary>
		[NotEmpty, NotLonger(128), JsonProperty]
		public virtual string Name { get; set; }

		/// <summary>
		/// Net server name or ip address, used in db connection string
		/// </summary>
		[NotEmpty, NotLonger(128)]
		public virtual string Server { get; set; }

		/// <summary>
		/// Provider name (PostgreSQL, System.Data.SqlClient only supported)
		/// </summary>
		[NotEmpty, NotLonger(128)]
		public virtual string Provider { get; set; }
	}
}