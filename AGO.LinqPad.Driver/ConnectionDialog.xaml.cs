using System.Windows;
using System.Xml.Linq;
using LINQPad.Extensibility.DataContext;

namespace AGO.LinqPad.Driver
{
    public partial class ConnectionDialog
    {
        public ConnectionDialog(IConnectionInfo cxInfo)
        {
            InitializeComponent();
            DataContext = new DriverDataWrapper(cxInfo.DriverData);
        }

        private void HandleOk(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }

    public class DriverDataWrapper
    {
        readonly XElement _DriverData;

        public DriverDataWrapper(XElement driverData)
        {
            _DriverData = driverData;
        }

		public string DisplayName
		{
			get { return (string)_DriverData.Element("DisplayName"); }
			set { _DriverData.SetElementValue("DisplayName", value); }
		}

        public string UserAssembliesFolder
        {
            get { return (string)_DriverData.Element("UserAssembliesFolder"); }
            set { _DriverData.SetElementValue("UserAssembliesFolder", value); }
        }

		public string ApplicationClass
		{
			get { return (string)_DriverData.Element("ApplicationClass"); }
			set { _DriverData.SetElementValue("ApplicationClass", value); }
		}
    }
}
