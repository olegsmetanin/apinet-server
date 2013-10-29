using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace AGO.LinqPad.Driver
{
	public class DataContext
	{
		protected DirectoryInfo _UserFolderInfo;
		protected readonly ISet<Assembly> _UserAssemblies = new HashSet<Assembly>();
		public virtual ISet<Assembly> UserAssemblies { get { return _UserAssemblies; } }

		protected readonly ISet<FileInfo> _UserAssemblyFiles = new HashSet<FileInfo>();
		public virtual ISet<FileInfo> UserAssemblyFiles { get { return _UserAssemblyFiles; } }

		protected DirectoryInfo _DriverFolderInfo;
		protected readonly ISet<FileInfo> _DriverAssemblyFiles = new HashSet<FileInfo>();
		public virtual ISet<FileInfo> DriverAssemblyFiles { get { return _DriverAssemblyFiles; } }

		protected readonly ISet<FileInfo> _AssemblyFiles = new HashSet<FileInfo>();
		public virtual ISet<FileInfo> AllAssemblyFiles { get { return _AssemblyFiles; } }

		protected DriverDataWrapper _DriverData;
		public DriverDataWrapper DriverData { get { return _DriverData; } }

		private IList<Type> _ExportedTypes = new List<Type>();
		public virtual IList<Type> ExportedTypes { get { return _ExportedTypes; } }

		public virtual void InitializeContext(IConnectionInfo cxInfo, string driverFolder)
		{
			if (cxInfo == null)
				throw new ArgumentNullException("cxInfo");
			if (string.IsNullOrWhiteSpace(driverFolder))
				throw new ArgumentNullException("driverFolder");

			_DriverData = new DriverDataWrapper(cxInfo.DriverData);
			_UserFolderInfo = new DirectoryInfo(_DriverData.UserAssembliesFolder);
			if (!_UserFolderInfo.Exists)
				throw new Exception(string.Format("Folder not found: {0}", _UserFolderInfo.FullName));
			_DriverFolderInfo = new DirectoryInfo(driverFolder);
			if (!_DriverFolderInfo.Exists)
				throw new Exception(string.Format("Folder not found: {0}", _DriverFolderInfo.FullName));

			DriverAssemblyFiles.UnionWith(_DriverFolderInfo.GetFiles("*.dll"));
			UserAssemblyFiles.UnionWith(_UserFolderInfo.GetFiles("*.dll").Where(fi => !"SQLite.Interop.dll".Equals(fi.Name)));

			UserAssemblies.UnionWith(UserAssemblyFiles.Where(file => !DriverAssemblyFiles.Any(driverFile =>
				driverFile.Name.Equals(file.Name, StringComparison.InvariantCultureIgnoreCase))).Select(file =>
				{
					try
					{
						return DataContextDriver.LoadAssemblySafely(file.FullName);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						return null;
					}
				}).Where(a => a != null));

			UserAssemblyFiles.Clear();
			UserAssemblyFiles.UnionWith(UserAssemblies.Select(
				a => new FileInfo(a.Location)).Where(fi => fi.Exists));

			AllAssemblyFiles.UnionWith(DriverAssemblyFiles);
			AllAssemblyFiles.UnionWith(UserAssemblyFiles);

			AppDomain.CurrentDomain.AssemblyResolve += (appDomain, args) =>
				UserAssemblies.FirstOrDefault(a => a.GetName().Name.Equals(args.Name) || a.FullName.Equals(args.Name));

			_ExportedTypes = new List<Type>(UserAssemblies.SelectMany(a => a.GetExportedTypes()));
		}

		public virtual void InitializeExecutionManager(QueryExecutionManager executionManager)
		{
		}
	}

	public class DynamicDataContext : DataContext
	{
		private readonly ISet<string> _AllNamespaces = new HashSet<string>();
		public virtual ISet<string> AllNamespaces { get { return _AllNamespaces; } }

		protected object _Application;

		public override void InitializeContext(IConnectionInfo cxInfo, string driverFolder)
		{
			base.InitializeContext(cxInfo, driverFolder);

			try
			{
				var configFiles = _UserFolderInfo.GetFiles("*.config").Select(file => file.FullName);
				var folder = _UserFolderInfo.Parent;

				if (folder != null)
					configFiles = configFiles.Union(folder.GetFiles("*.config").Select(file => file.FullName));

				var addElements = configFiles.SelectMany(
					file => GetAppSettingsNodeFromXmlFile(file).Elements("add"));
				var appSettings = new Dictionary<string, string>();
				foreach (var element in addElements)
				{
					var keyElement = element.Attribute("key");
					var valueElement = element.Attribute("value");				

					var key = keyElement != null ? keyElement.Value.Trim() : string.Empty;
					var value = valueElement != null ? valueElement.Value : string.Empty;
					if (string.IsNullOrEmpty(key))
						continue;

					if (key.Equals("Hibernate_AutoMappingAssemblies"))
					{
						if (!string.IsNullOrWhiteSpace(value))
							value += ";";
						value += GetType().Assembly.GetName().Name;
					}
					appSettings[key] = value;
				}

				_Application = Activator.CreateInstance(GetExportedType(_DriverData.ApplicationClass, true, true));

				var setter = _Application.GetType().GetProperty("KeyValueProvider");
				if (setter != null && setter.CanWrite)
						setter.SetValue(_Application, Activator.CreateInstance(GetExportedType(
					"AGO.Core.Config.DictionaryKeyValueProvider", true, true), new object[] { appSettings }), null);
			}
			catch (TargetInvocationException e)
			{
				throw e.InnerException ?? e;
			}
		}

		#region Helper methods

		protected XElement GetAppSettingsNodeFromXmlFile(string xmlFilePath)
		{
			XElement result = null;
			try
			{
				result = XDocument.Load(xmlFilePath).Element("configuration");
				result = result != null ? result.Element("appSettings") : null;
			}
			catch (Exception e)
			{
				Console.WriteLine("Ошибка загрузки настроек из файла {0}\n{1}", xmlFilePath, e);
			}
			return result ?? new XElement("empty");
		}

		protected Type GetExportedType(string name, bool throwOnNull = false, bool fullName = false)
		{
			var result = ExportedTypes.FirstOrDefault(t => fullName
				? t.FullName.Equals(name, StringComparison.InvariantCulture)
				: t.Name.Equals(name, StringComparison.InvariantCulture));
			if (throwOnNull && result == null)
				throw new Exception(string.Format("Type \"{0}\" is not found", name));
			return result;
		}

		#endregion
	}
}