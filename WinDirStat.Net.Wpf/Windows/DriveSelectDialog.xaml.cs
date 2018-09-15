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
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.ViewModel;
using WinDirStat.Net.Wpf.Utils;

namespace WinDirStat.Net.Wpf.Windows {
	/// <summary>
	/// Interaction logic for DriveSelectDialog.xaml
	/// </summary>
	public partial class DriveSelectDialog : Window {

		#region Constructors

		/// <summary>Constructs the <see cref="DriveSelectDialog"/>.</summary>
		private DriveSelectDialog() {
			InitializeComponent();
		}

		#endregion

		#region Properties

		/// <summary>Gets the <see cref="DriveSelectViewModel"/>.</summary>
		public DriveSelectViewModel ViewModel  => DataContext as DriveSelectViewModel;

		#endregion

		#region Event Handlers

		private void OnLoaded(object sender, RoutedEventArgs e) {
			ViewModel.WindowOwner = this;
			list.Focus();
			Keyboard.Focus(list);
		}

		private void OnSourceInitialized(object sender, EventArgs e) {
			this.ShowMaximizeMinimize(false, false);
		}

		private void OnOK(object sender, RoutedEventArgs e) {
			DialogResult = true;
		}

		#endregion

		#region ShowDialog

		/// <summary>Shows the drive select dialog.</summary>
		/// 
		/// <param name="owner">The owner window of this dialog.</param>
		/// <returns>The selected root paths on success, otherwise null.</returns>
		public static DriveSelectResult ShowDialog(Window owner) {
			DriveSelectDialog dialog = new DriveSelectDialog {
				Owner = owner,
			};
			bool? result = dialog.ShowDialog();
			if (result ?? false)
				return dialog.ViewModel.Result;
			return null;
		}

		#endregion
	}
}
