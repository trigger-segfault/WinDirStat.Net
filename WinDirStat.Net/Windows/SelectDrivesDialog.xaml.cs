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
using WinDirStat.Net.SortingView;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Windows {
	/// <summary>
	/// Interaction logic for SelectDrivesDialog.xaml
	/// </summary>
	public partial class SelectDrivesDialog : Window {

		private readonly SelectDrivesDocument document;

		public SelectDrivesDialog(SelectDrivesDocument document) {
			this.document = document;
			InitializeComponent();
			DataContext = document;
		}

		private void OnSourceInitialized(object sender, EventArgs e) {
			this.ShowMaximizeMinimize(false, false);
		}

		private void OnOK(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}

		private void OnDriveSelectionChanged(object sender, SelectionChangedEventArgs e) {
			document.SetSelectedDrives(list.SelectedItems.Cast<SelectDrive>());
		}
		
		private void OnBrowseFolder(object sender, RoutedEventArgs e) {
			FolderBrowserDialog dialog = new FolderBrowserDialog() {
				Description	= "WinDirStat.Net - Select Folder",
				ShowNewFolderButton = false,
			};
			bool? result = dialog.ShowDialog(this);
			if (result ?? false) {
				document.FolderPath = dialog.SelectedPath;
			}
		}

		private void OnDriveListGotFocus(object sender, RoutedEventArgs e) {
			document.SelectMode = SelectDriveMode.Individual;
		}

		private void OnLoaded(object sender, RoutedEventArgs e) {
			document.Drives.Refresh();
			if (document.Drives.Count > 0)
				list.SelectedItem = document.Drives[0];
		}

		private void OnSortDrives(object sender, SortViewEventArgs e) {
			document.SetDriveSort(e.ParseMode<SelectDriveSortMethod>(), e.Direction);
		}

		public bool? ShowDialog(Window owner) {
			Owner = owner;
			return ShowDialog();
		}
	}
}
