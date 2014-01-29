using System;
using System.Collections.Generic;
using System.Linq;

namespace AGO.Reporting.Service
{
	/// <summary>
	/// Вспомогательный класс для выбора каждый раз нового проекта (если их несколько). 
	/// Запоминает последний отданный проект с целью выдать при следующем вызове другой.
	/// </summary>
	public class ProjectSelector
	{
		private readonly string singleProject;

		private readonly string[] someProjects;
		private int lastProjectIndex = -1;

		private string lastProject;

		public ProjectSelector(string projectMask)
		{
			if (string.IsNullOrWhiteSpace(projectMask))
				throw new ArgumentNullException("projectMask");

			if ("*" == projectMask)
			{
				//all projects, will be selected from external source
			}
			else
			{
				var parts = projectMask.Split(',');
				if (parts.Length > 1)
				{
					someProjects = parts.Select(p => p.Trim()).ToArray();
				}
				else
				{
					singleProject = projectMask;
				}
			}
		}

		public string NextProject(Func<IEnumerable<string>> availableProjects)
		{
			if (singleProject != null) return singleProject;

			if (someProjects != null)
			{
				lastProjectIndex = lastProjectIndex == -1 || lastProjectIndex == someProjects.Length - 1
					? 0
					: lastProjectIndex + 1;
				return someProjects[lastProjectIndex];
			}

			if (availableProjects == null) return string.Empty;
			var ap = availableProjects().ToArray();
			if (ap.Length <= 0) return string.Empty;

			lastProject = ap.FirstOrDefault(p => p != lastProject) ?? lastProject ?? string.Empty;
			return lastProject;
		}
	}
}