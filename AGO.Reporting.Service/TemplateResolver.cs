using System;
using System.IO;
using System.Threading;
using AGO.Core;
using AGO.Reporting.Common.Model;

namespace AGO.Reporting.Service
{
	public class TemplateResolver
	{
		private readonly string templatesCacheDirectory;
		private readonly ReaderWriterLockSlim tlock;

		public TemplateResolver(string templatesPath)
		{
			if (templatesPath.IsNullOrWhiteSpace())
				throw new ArgumentNullException("templatesPath");

			templatesCacheDirectory = templatesPath;
			tlock = new ReaderWriterLockSlim();
		}

		public string Resolve(IReportTemplate template)
		{
			if (template != null)
			{
				//Этот код будет создавать директорию очень редко, обычно при первом старте сервиса,
				//если папку забыли создать вручную. Поэтому синхронизация не делается.
				if (!Directory.Exists(templatesCacheDirectory))
				{
					Directory.CreateDirectory(templatesCacheDirectory);
				}

				tlock.EnterUpgradeableReadLock();
				try
				{
					var tmpTemplateFile = Path.Combine(templatesCacheDirectory, template.Name);
					if (File.Exists(tmpTemplateFile) && File.GetCreationTime(tmpTemplateFile) >= template.LastChange)
					{
						//Файл шаблона уже сброшен во временную папку, и его версия актуальна
						return tmpTemplateFile;
					}

					tlock.EnterWriteLock();
					try
					{
						//Безопасно вызывать, даже если файла нет. Просто сюда можно попасть по 2-м условиям:
						//файл устарел или файл отсутствует
						File.Delete(tmpTemplateFile);
						using (var fs = File.Create(tmpTemplateFile))
						{
							fs.Write(template.Content, 0, template.Content.Length);
							fs.Close();
						}
					}
					finally
					{
						tlock.ExitWriteLock();
					}
					return tmpTemplateFile;
				}
				finally
				{
					tlock.ExitUpgradeableReadLock();
				}
			}
			return null;
		}
	}
}