using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;
using Microsoft.CSharp;

namespace AGO.LinqPad.Driver
{
	public class DynamicDataContextDriver : LINQPad.Extensibility.DataContext.DynamicDataContextDriver
	{
		internal static object _Lock = new object();

		public const string DriverName = "AGO.LinqPad.Driver";

		public const string AuthorName = "GeX";

		public override string GetConnectionDescription(IConnectionInfo cxInfo)
		{
			var driverData = new DriverDataWrapper(cxInfo.DriverData);
			return string.IsNullOrWhiteSpace(driverData.DisplayName) 
				? DriverName : DriverName + ": " + driverData.DisplayName;
		}

		public override string Name
		{
			get { return DriverName; }
		}

		public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
		{
			cxInfo.CustomTypeInfo.CustomTypeName = typeof(DynamicDataContext).FullName;
			cxInfo.CustomTypeInfo.CustomAssemblyPath = Assembly.GetExecutingAssembly().Location;
			return new ConnectionDialog(cxInfo).ShowDialog().GetValueOrDefault();
		}

		public override string Author
		{
			get { return AuthorName; }
		}

		public override bool AreRepositoriesEquivalent(IConnectionInfo c1, IConnectionInfo c2)
		{
			return XNode.DeepEquals(c1.DriverData, c2.DriverData);
		}

		public override IEnumerable<string> GetNamespacesToAdd(IConnectionInfo cxInfo)
		{
			lock (_Lock)
			{
				var ctx = new DynamicDataContext();
				ctx.InitializeContext(cxInfo, GetDriverFolder());
				return ctx.AllNamespaces;
			}
		}

		public override IEnumerable<string> GetAssembliesToAdd(IConnectionInfo cxInfo)
		{
			lock (_Lock)
			{
				var ctx = new DataContext();
				ctx.InitializeContext(cxInfo, GetDriverFolder());
				return ctx.AllAssemblyFiles.Select(f => f.FullName);
			}
		}

		public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
		{
			var dynamicContext = context as DynamicDataContext;
			if (dynamicContext == null)
				return;

			dynamicContext.InitializeContext(cxInfo, GetDriverFolder());
			dynamicContext.InitializeExecutionManager(executionManager);

			base.InitializeContext(cxInfo, context, executionManager);
		}

		public override List<ExplorerItem> GetSchemaAndBuildAssembly(
			IConnectionInfo cxInfo, AssemblyName assemblyToBuild, ref string nameSpace, ref string typeName)
		{
			lock (_Lock)
			{
				var ctx = new DynamicDataContext();
				ctx.InitializeContext(cxInfo, GetDriverFolder());
				BuildAssembly(ctx, assemblyToBuild, ref nameSpace, ref typeName);
				return new List<ExplorerItem>();
			}
		}

		public virtual void BuildAssembly(
			DynamicDataContext ctx,
			AssemblyName assemblyToBuild,
			ref string nameSpace, ref string typeName)
		{
			var code = string.Format(@"
				using AGO.LinqPad.Driver;

				namespace {0}
				{{
					public class {1} : DynamicDataContext
					{{
						public virtual {2} Application
						{{
							get
							{{
								return _Application as {2};
							}}
						}}
					}}
				}}
			", nameSpace, typeName, ctx.DriverData.ApplicationClass);

			CompilerResults results;
			using (var codeProvider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v3.5" } }))
			{
				var options = new CompilerParameters(
					ctx.AllAssemblyFiles.Select(f => f.FullName).ToArray(),
					assemblyToBuild.CodeBase,
					true);
				results = codeProvider.CompileAssemblyFromSource(options, code);
			}

			if (results.Errors.Count == 0)
				return;

			throw new Exception(string.Format("Не удалось скомпилировать динамический контекст: {0}\nошибка в строке {1}",
					results.Errors[0].ErrorText, results.Errors[0].Line));
		}
	}
}