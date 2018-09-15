using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WinDirStat.Net.Wpf.Utils;

namespace WinDirStat.Net.Wpf.Windows {
	/// <summary>
	/// Interaction logic for ConfigureDialog.xaml
	/// </summary>
	public partial class ConfigureDialog : Window {
		public ConfigureDialog() {
			InitializeComponent();
		}

		#region Properties

		/// <summary>Gets the <see cref="ConfigureViewModel"/>.</summary>
		//public ConfigureViewModel ViewModel => DataContext as ConfigureViewModel;

		#endregion

		#region Event Handlers

		private void OnLoaded(object sender, RoutedEventArgs e) {
			//ViewModel.WindowOwner = this;
		}

		private void OnSourceInitialized(object sender, EventArgs e) {
			this.ShowMaximizeMinimize(false, false);
		}

		private void OnOK(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}

		#endregion
	}
}
