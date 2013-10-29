using System.Windows;
using System.Windows.Forms;

namespace AGO.LinqPad.Driver
{
	/// <summary>
	/// Interaction logic for FolderEntry.xaml
	/// </summary>
	public partial class FolderEntry
	{
		public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FolderEntry), 
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(FolderEntry), 
			new PropertyMetadata(null));

		public string Text { get { return GetValue(TextProperty) as string; } set { SetValue(TextProperty, value); } }

		public string Description { get { return GetValue(DescriptionProperty) as string; } set { SetValue(DescriptionProperty, value); } }

		public FolderEntry() { InitializeComponent(); }

		private void BrowseFolder(object sender, RoutedEventArgs e)
		{
			using (var dlg = new FolderBrowserDialog())
			{
				dlg.Description = Description;
				dlg.SelectedPath = Text;
				dlg.ShowNewFolderButton = true;
				var result = dlg.ShowDialog();
				if (result != DialogResult.OK)
					return;
				Text = dlg.SelectedPath;
				var be = GetBindingExpression(TextProperty);
				if (be != null)
					be.UpdateSource();
			}
		}
	}
}
