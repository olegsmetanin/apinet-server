using System;
using System.Collections.Generic;
using AGO.Core.Model.Projects;
using Newtonsoft.Json;

namespace AGO.Core.Controllers.Projects
{
	public class ProjectViewModel
	{
		[JsonProperty]
		public ProjectModel Model { get; private set; }

		[JsonProperty]
		public ISet<LookupEntry> Tags { get { return _Tags; } set { _Tags = value; } }
		private ISet<LookupEntry> _Tags = new HashSet<LookupEntry>();

		public ProjectViewModel(ProjectModel model)
		{
			if (model == null)
				throw new ArgumentNullException("model");
			Model = model;
		}
	}
}