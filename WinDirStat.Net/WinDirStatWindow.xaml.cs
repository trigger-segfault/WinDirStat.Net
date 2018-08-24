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
using WinDirStat.Net.Model.Data.Nodes;
using WinDirStat.Net.Model.View;
using WinDirStat.Net.Model.View.Extensions;
using WinDirStat.Net.Model.View.Nodes;
using WinDirStat.Net.Windows;

namespace WinDirStat.Net {
	/// <summary>
	/// Interaction logic for WinDirWindow.xaml
	/// </summary>
	public partial class WinDirStatWindow : Window {
		public WinDirStatWindow() {
			InitializeComponent();
		}

		private void OnDriveSelect(object sender, DriveSelectEventArgs e) {
			DriveSelectDialog.ShowDialog(this, viewModel, e);
		}

		private void OnFileSelectionChanged(object sender, SelectionChangedEventArgs e) {
			OnFileTreeGotFocus(sender, e);
		}

		private void OnExtensionSelectionChanged(object sender, SelectionChangedEventArgs e) {
			OnExtensionListGotFocus(sender, e);
		}

		private void OnFileTreeGotFocus(object sender, RoutedEventArgs e) {
			if (tree.SelectedItems.Count > 0 && viewModel.IsFinished) {
				graphView.HighlightSelection(tree.SelectedItems.Cast<FileNodeViewModel>().Select(v => v.Model));
			}
		}

		private void OnExtensionListGotFocus(object sender, RoutedEventArgs e) {
			if (extensionList.SelectedItem != null && viewModel.IsFinished) {
				graphView.HighlightExtension(((ExtensionRecordViewModel) extensionList.SelectedItem).Extension);
			}
		}
		private void OnGraphFileSelected(object sender, MouseButtonEventArgs e) {
			if (graphView.HasHover) {
				FileNodeViewModel view = viewModel.RootNode.FindView(graphView.Hover, true);
				tree.FocusNode(view);
				tree.SelectedItem = view;
				//tree.Focus();
				//Keyboard.Focus(tree);
			}
		}
	}
}
