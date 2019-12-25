using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace OpenKh.Tools.Common.Dialogs
{
	/// <summary>
	/// Interaction logic for AboutDialog.xaml
	/// </summary>
	public partial class AboutDialog : Window, INotifyPropertyChanged
	{
		private string name;
		private string version;
		private string website;
		private string author;
		private string powered;
		private string authorwebsite;

		public AboutDialog(Assembly assembly = null)
		{
			InitializeComponent();

			assembly = assembly ?? Assembly.GetExecutingAssembly();
			var assemblyName = assembly.GetName();
			var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

			name = fvi.ProductName;
			website = fvi.Comments;
			version = $"compiled date time or git hash and branch?";
			author = "OpenKh blabla";
			powered = "blablablabla";
			authorwebsite = "https://openkh.dev";

			DataContext = this;
			MouseDown += (o, e) =>
			{
				if (e.ChangedButton == MouseButton.Left)
					DragMove();
			};
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string ToolName
		{
			get => name;
			set => name = OnPropertyChanged(value);
		}

		public string Version
		{
			get => version;
			set => version = OnPropertyChanged(value);
		}

		public string Website
		{
			get => website;
			set => website = OnPropertyChanged(value);
		}

		public string Author
		{
			get => author;
			set => author = OnPropertyChanged(value);
		}

		public string PoweredBy
		{
			get => powered;
			set => powered = OnPropertyChanged(value);
		}

		public string AuthorWebsite
		{
			get => authorwebsite;
			set => authorwebsite = OnPropertyChanged(value);
		}

		private T OnPropertyChanged<T>(T obj, [CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			return obj;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			BeginAnimation(OpacityProperty, new DoubleAnimation()
			{
				From = 0.0,
				To = 1.0,
				EasingFunction = new CubicEase(),
				Duration = new Duration(TimeSpan.FromSeconds(0.5))
			});
		}
	}
}
