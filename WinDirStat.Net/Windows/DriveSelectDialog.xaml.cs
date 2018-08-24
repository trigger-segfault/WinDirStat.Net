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
using WinDirStat.Net.Model.View;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Windows {
	/// <summary>
	/// Interaction logic for DriveSelectDialog.xaml
	/// </summary>
	public partial class DriveSelectDialog : Window {
		private DriveSelectDialog() {
			InitializeComponent();
		}

		public DriveSelectViewModel ViewModel {
			get => viewModel;
		}

		private void OnSourceInitialized(object sender, EventArgs e) {
			this.ShowMaximizeMinimize(false, false);
		}

		private void OnOK(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}

		public static void ShowDialog(Window owner, WinDirStatViewModel viewModel, DriveSelectEventArgs e) {
			DriveSelectDialog dialog = new DriveSelectDialog();
			dialog.Owner = owner;
			dialog.viewModel.Initialize(viewModel, e);
			e.Result = dialog.ShowDialog() ?? false;
			if (e.Result) {
				e.Mode = dialog.viewModel.Mode;
				e.SelectedDrives = dialog.viewModel.SelectedDriveNames;
				e.FolderPath = dialog.viewModel.FolderPath;
				switch (e.Mode) {
				case DriveSelectMode.All:
					e.ResultPaths = dialog.viewModel.DriveNames;
					break;
				case DriveSelectMode.Individual:
					e.ResultPaths = dialog.viewModel.SelectedDriveNames;
					break;
				case DriveSelectMode.Folder:
					e.ResultPaths = new[] { dialog.viewModel.FolderPath };
					break;
				}
			}
		}

		private void OnBrowseFolder(object sender, RoutedEventArgs e) {
			FolderBrowserDialog dialog = new FolderBrowserDialog() {
				Description = "WinDirStat.Net - Select Folder",
				ShowNewFolderButton = false,
			};
			bool? result = dialog.ShowDialog(this);
			if (result ?? false) {
				viewModel.FolderPath = dialog.SelectedPath;
			}
		}
	}
}
