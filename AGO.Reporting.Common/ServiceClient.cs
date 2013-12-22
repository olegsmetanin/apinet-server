using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;


namespace AGO.Reporting.Common
{
	public class ServiceClient: IReportingService
	{
		private const string JSON_TYPE = "application/json";
		private const string ERROR_JSON_REGEX = @"^\{message:(?<err>.*)\}";

		private readonly string endpoint;
		private readonly List<WebRequest> runningRequests;

		public ServiceClient(string ep)
		{
			if (string.IsNullOrWhiteSpace(ep))
				throw new ArgumentNullException("ep");
			if (!Uri.IsWellFormedUriString(ep, UriKind.Absolute))
				throw new ArgumentException("Endpoint is not wellformed uri", "ep");

			endpoint = ep;
			runningRequests = new List<WebRequest>();
		}

		public void Dispose()
		{
			foreach (var request in runningRequests)
			{
				request.Abort();
			}
			runningRequests.Clear();
		}

		private string Call(string method, string json = null)
		{
			var request = WebRequest.Create(endpoint + "/api/" + method);
			request.UseDefaultCredentials = true;
			var payload = "{" + json + "}";
			request.ContentType = JSON_TYPE;
			request.Method = WebRequestMethods.Http.Post;
			var buffer = Encoding.UTF8.GetBytes(payload);
			request.ContentLength = buffer.Length;
			using (var s = request.GetRequestStream())
			{
				s.Write(buffer, 0, buffer.Length);
				s.Close();
			}
			try
			{
				using (var response = request.GetResponse())
				{
					string result = null;
					using (var s = response.GetResponseStream())
					{
						if (s != null)
						{
							result = new StreamReader(s, Encoding.UTF8).ReadToEnd().Trim();
							s.Close();
						}
					}
					response.Close();

					if (!string.IsNullOrWhiteSpace(result))
					{
						var m = Regex.Match(result, ERROR_JSON_REGEX);
						if (m.Success)
						{
							throw new ReportingClientException(m.Groups["err"].Value);
						}
					}

					return result;
				}
			}
			catch(ReportingClientException)
			{
				throw;
			}
			catch (Exception ex)
			{
				var rex = new ReportingClientException("Error on reporting service call", ex);
				rex.Data["Method"] = method;
				rex.Data["Payload"] = payload;
				rex.Data["Endpoint"] = endpoint;
				throw rex;
			}
		}

		public bool Ping()
		{
			try
			{
				return bool.Parse(Call("Ping"));
			}
			catch (Exception)
			{
				return false;
			}
		}

		public void RunReport(Guid taskId)
		{
			Call("RunReport", "taskId: '" + taskId + "'");
		}

		public bool CancelReport(Guid taskId)
		{
			return bool.Parse(Call("CancelReport", "taskId: '" + taskId + "'"));
		}

		public bool IsRunning(Guid taskId)
		{
			return bool.Parse(Call("IsRunning", "taskId: '" + taskId + "'"));
		}

		public bool IsWaitingForRun(Guid taskId)
		{
			return bool.Parse(Call("IsWaitingForRun", "taskId: '" + taskId + "'"));
		}
	}
}