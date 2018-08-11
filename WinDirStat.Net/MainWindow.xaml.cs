using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WinDirStat.Net.Controls;
using WinDirStat.Net.Data;
using WinDirStat.Net.Data.Nodes;
using WinDirStat.Net.SortingView;
using WinDirStat.Net.TreeView;
using WinDirStat.Net.Utils;
using WinDirStat.Net.Windows;

namespace WinDirStat.Net {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		private WinDirDocument document;
		
		public static void InstanceOverheadTest<T>(int size = 100000) where T : new() {
			object[] array = new object[size];
			long initialMemory = GC.GetTotalMemory(true);
			for (int i = 0; i < size; i++) {
				array[i] = new T();
			}
			long finalMemory = GC.GetTotalMemory(true);
			GC.KeepAlive(array);
			long total = finalMemory - initialMemory;
			double averageSize = (double) total / size;
			Console.WriteLine($"{typeof(T).Name} average size: {averageSize:0.000} bytes");
		}

		public static void InstanceOverheadTest<T>(Func<T> construct, int size = 100000) {
			object[] array = new object[size];
			long initialMemory = GC.GetTotalMemory(true);
			for (int i = 0; i < size; i++) {
				array[i] = construct();
			}
			long finalMemory = GC.GetTotalMemory(true);
			GC.KeepAlive(array);
			long total = finalMemory - initialMemory;
			double averageSize = (double) total / size;
			Console.WriteLine($"{typeof(T).Name} average size: {averageSize:0.000} bytes");
		}

		public MainWindow() {
			FileInfo fileInfo = new FileInfo(@"C:\fragmentation.txt");
			DirectoryInfo dirInfo = new DirectoryInfo(@"C:\fragmentation.txt");
			InstanceOverheadTest<FileNode>(() => new FileNode(fileInfo));
			InstanceOverheadTest<FolderNode>(() => new FolderNode(dirInfo));
			//Environment.Exit(0);


			document = new WinDirDocument();
			document.ScanEnded += OnScanEnded;

			InitializeComponent();

			DataContext = document;

			tree.ShowRootExpander = false;
			tree.ShowRoot = true;
			
			StartScan(@"C:\");
		}

		private void OnScanEnded(object sender, ScanEventArgs e) {
			switch (e.ScanState) {
			case ScanState.Finished:
				//graphView.Root = document.RootNode;
				//extensionList.ItemsSource = document.Extensions;
				break;
			case ScanState.Failed:
				Console.WriteLine(e.Exception);
				break;
			}
		}

		private RootNode Root => tree.Root;

		private void StartScan(string rootPath) {
			//graphView.Root = null;
			//extensionList.ItemsSource = null;
			document.ScanAsync(rootPath);
		}

		/*[DllImport("psapi.dll")]
		static extern int EmptyWorkingSet(IntPtr hwProc);

		static void MinimizeFootprint() {
			EmptyWorkingSet(Process.GetCurrentProcess().Handle);
		}*/

		private void OnClosing(object sender, CancelEventArgs e) {
			document.Dispose();
		}

		private void OnFileSort(object sender, SortViewEventArgs e) {
			document.SetFileSort(e.ParseMode<FileSortMethod>(), e.Direction);
		}

		private void OnExtensionSort(object sender, SortViewEventArgs e) {
			document.SetExtensionSort(e.ParseMode<ExtensionSortMethod>(), e.Direction);
		}

		private void OnGraphFileSelected(object sender, MouseButtonEventArgs e) {
			if (graphView.HasHover) {
				tree.FocusNode(graphView.Hover);
				tree.SelectedItem = graphView.Hover;
				tree.Focus();
			}
		}

		private void OnCancelScan(object sender, RoutedEventArgs e) {
			document.Cancel(false);
		}

		private void OnOpen(object sender, RoutedEventArgs e) {
			bool suspended = false;
			if (document.IsScanningOrRefreshing && !document.IsSuspended)
				document.IsSuspended = suspended = true;
			FolderBrowserDialog dialog = new FolderBrowserDialog() {
				ShowNewFolderButton = false,
				Description = "Select a folder to scan",
			};
			bool? result = dialog.ShowDialog(this);
			if (result ?? false) {
				document.CancelAsync(() => StartScan(dialog.SelectedPath), true);
			}
			else if (suspended) {
				document.IsSuspended = false;
			}
		}

		private void OnExit(object sender, RoutedEventArgs e) {
			Close();
		}

		private void OnFileSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (tree.SelectedItems != null && tree.SelectedItems.Count > 0 && document.IsFinished) {
				graphView.HighlightSelection(tree.SelectedItems.Cast<FileNode>());
			}
		}

		private void OnExtensionSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (extensionList.SelectedItem != null && document.IsFinished) {
				graphView.HighlightExtension(((ExtensionRecord) extensionList.SelectedItem).Extension);
			}
		}
	}
}
