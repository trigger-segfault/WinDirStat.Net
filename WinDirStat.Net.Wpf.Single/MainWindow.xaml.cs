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
using System.Windows.Navigation;
using WinDirStat.Net.ViewModel;
using WinDirStat.Net.ViewModel.Extensions;
using WinDirStat.Net.ViewModel.Files;

namespace WinDirStat.Net.Wpf {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		#region Constructors

		/// <summary>Constructs the <see cref="DriveSelectDialog"/>.</summary>
		public MainWindow() {
			InitializeComponent();
		}

		#endregion

		#region Properties

		/// <summary>Gets the <see cref="MainViewModel"/>.</summary>
		public MainViewModel ViewModel => DataContext as MainViewModel;

		#endregion

		#region Event Handlers

		private void OnLoaded(object sender, RoutedEventArgs e) {
			ViewModel.WindowOwner = this;
		}

		private void OnFileSelectionChanged(object sender, SelectionChangedEventArgs e) {
			OnFileTreeGotFocus(sender, e);
		}

		private void OnExtensionSelectionChanged(object sender, SelectionChangedEventArgs e) {
			OnExtensionListGotFocus(sender, e);
		}

		private void OnFileTreeGotFocus(object sender, RoutedEventArgs e) {
			if (tree.SelectedItems.Count > 0 && ViewModel.IsOpen) {
				graphView.HighlightSelection(tree.SelectedItems.Cast<FileItemViewModel>().Select(v => v.Model));
			}
		}

		private void OnExtensionListGotFocus(object sender, RoutedEventArgs e) {
			if (extensionList.SelectedItem != null && ViewModel.IsOpen) {
				graphView.HighlightExtension(((ExtensionItemViewModel) extensionList.SelectedItem).Extension);
			}
		}
		private void OnGraphFileSelected(object sender, MouseButtonEventArgs e) {
			if (graphView.HasHover) {
				FileItemViewModel view = ViewModel.RootItem.FindView(graphView.Hover, true);
				tree.FocusNode(view);
				tree.SelectedItem = view;
				//tree.Focus();
				//Keyboard.Focus(tree);
			}
		}

		#endregion
	}
}
