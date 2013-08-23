using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using AGO.Hibernate;
using AGO.Hibernate.Json;
using Newtonsoft.Json;

namespace AGO.WebApiApp.Controllers
{
	public class ApiController : Controller
	{
		public ActionResult Dispatch()
		{			
			try
			{
				var serviceType = RouteData.Values["serviceType"] as Type;
				if (serviceType == null)
					throw new Exception("serviceType is null");

				var method = RouteData.Values["method"] as MethodInfo;
				if (method == null)
					throw new Exception("method is null");
				
				var jsonService = DependencyResolver.Current.GetService<IJsonService>();
				JsonReader inputReader = null;
				JsonWriter outputWriter = null;
				var stringBuilder = new StringBuilder();

				var parameters = new List<object>();
				foreach (var parameterInfo in method.GetParameters())
				{
					if (typeof (JsonReader).IsAssignableFrom(parameterInfo.ParameterType))
					{
						inputReader = inputReader ?? 
							jsonService.CreateReader(new StreamReader(HttpContext.Request.InputStream, true));
						parameters.Add(inputReader);
						continue;
					}
					if (typeof (JsonWriter).IsAssignableFrom(parameterInfo.ParameterType))
					{
						outputWriter = outputWriter ?? 
							jsonService.CreateWriter(new StringWriter(stringBuilder), true);
						parameters.Add(outputWriter);
						continue;
					}
					parameters.Add(null);
				}

				if (inputReader == null || outputWriter == null)
					throw new Exception("Invalid service method signature");

				var service = DependencyResolver.Current.GetService(serviceType);
				var initializable = service as IInitializable;
				if (initializable != null)
					initializable.Initialize();

				try
				{
					method.Invoke(DependencyResolver.Current.GetService(serviceType), parameters.ToArray());
				}
				catch (TargetInvocationException e)
				{
					throw e.InnerException;
				}

				outputWriter.Flush();
				return Content(stringBuilder.ToString(), "application/json", Encoding.UTF8);
			}
			catch (Exception e)
			{
				return Json(new { result = "error", message = e.Message });
			}
		}
	}
}