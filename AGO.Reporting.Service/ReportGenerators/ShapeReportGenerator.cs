using System.IO;
using System.Threading;
using System.Xml;
using AGO.Reporting.Common;

namespace AGO.Reporting.Service.ReportGenerators
{
    /// <summary>
    /// Каркас для генератора отчетов, выдающего данные в формате ????
    /// </summary>
    public class ShapeReportGenerator: BaseReportGenerator, IReportGenerator
    {
        #region IReportGenerator Members

        public void MakeReport(string pathToTemplate, XmlDocument data, CancellationToken token)
        {
        	CancellationToken = token;
            throw new System.NotImplementedException();
        }

        #endregion

        #region IReportGeneratorResult Members

        public Stream Result
        {
            get { throw new System.NotImplementedException(); }
        }

        public string GetFileName(string proposed)
        {
            throw new System.NotImplementedException();
        }

    	public string ContentType
    	{
    		get { throw new System.NotImplementedException(); }
    	}

    	#endregion
    }
}