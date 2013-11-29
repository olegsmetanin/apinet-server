using System.IO;
using System.Xml;
using AGO.Reporting.Common;

namespace AGO.Reporting.Service
{
    /// <summary>
    /// Каркас для генератора отчетов, выдающего данные в формате ????
    /// </summary>
    public class ShapeReportGenerator: IReportGenerator
    {
        #region IReportGenerator Members

        public void MakeReport(string pathToTemplate, XmlDocument data)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region IReportGeneratorResult Members

        public Stream Result
        {
            get { throw new System.NotImplementedException(); }
        }

        public string FileName
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        #endregion
    }
}