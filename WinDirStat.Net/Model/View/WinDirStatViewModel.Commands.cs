using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WinDirStat.Net.Model.Data.Nodes;
using WinDirStat.Net.Resources;
using WinDirStat.Net.SortingView;

namespace WinDirStat.Net.Model.View {
	partial class WinDirStatViewModel {
		
		#region File Menu

		public ICommand Open {
			get => GetCommand(new RelayUICommand("Open", Images.Open,
				new KeyGesture(Key.O, ModifierKeys.Control), OnOpen));
		}
		public ICommand Save {
			get => GetCommand(new RelayUICommand("Save", Images.Save,
				new KeyGesture(Key.S, ModifierKeys.Control), OnSave, CanExecuteIsFinished));
		}
		public ICommand Reload {
			get => GetCommand(new RelayUICommand("Reload", Images.Reload,
				new KeyGesture(Key.F5, ModifierKeys.Shift), OnReload, CanExecuteIsFinished));
		}
		public ICommand Close {
			get => GetCommand(new RelayUICommand("Close", Images.DeleteFolder,
				OnClose, CanExecuteIsFinished));
		}
		public ICommand Elevate {
			get => GetCommand(new RelayUICommand("Elevate", Images.Elevate, OnElevate));
		}
		public ICommand Exit {
			get => GetCommand(new RelayUICommand("Exit", Images.Exit,
				new KeyGesture(Key.W, ModifierKeys.Control), OnExit));
		}

		#endregion

		#region Context Menu/Toolbar

		public ICommand Expand {
			get => GetCommand(new RelayUICommand("Expand", Images.Expand,
				OnExpand, CanExecuteExpand));
		}
		public ICommand Collapse {
			get => GetCommand(new RelayUICommand("Collapse", Images.Expand,
				OnExpand, CanExecuteExpand));
		}
		public ICommand OpenItem {
			get => GetCommand(new RelayUICommand("Open Item", Images.Run,
				new KeyGesture(Key.Return), OnOpenItem, CanExecuteIsSingleSelectionFileType));
		}
		public ICommand CopyPath {
			get => GetCommand(new RelayUICommand("Copy Path", Images.Path,
				new KeyGesture(Key.C, ModifierKeys.Control), OnCopyPath, CanExecuteIsSingleSelectionFileType));
		}
		public ICommand Explore {
			get => GetCommand(new RelayUICommand("Explore Here", Images.Explore,
				new KeyGesture(Key.C, ModifierKeys.Control), OnExplore, CanExecuteIsSingleSelection));
		}
		public ICommand CommandPrompt {
			get => GetCommand(new RelayUICommand("Command Prompt", Images.Cmd,
				new KeyGesture(Key.P, ModifierKeys.Control), OnCommandPrompt,
				CanExecuteIsSingleSelectionFileType));
		}
		public ICommand CommandPromptElevated {
			get => GetCommand(new RelayUICommand("Elevated Command Prompt", Images.CmdElevated,
				new KeyGesture(Key.P, ModifierKeys.Control | ModifierKeys.Shift), OnCommandPromptElevated,
				CanExecuteIsSingleSelectionFileType));
		}
		public ICommand PowerShell {
			get => GetCommand(new RelayUICommand("PowerShell", Images.PowerShell,
				new KeyGesture(Key.P, ModifierKeys.Control), OnCommandPrompt,
				CanExecuteIsSingleSelectionFileType));
		}
		public ICommand PowerShellElevated {
			get => GetCommand(new RelayUICommand("Elevated PowerShell", Images.PowerShellElevated,
				new KeyGesture(Key.P, ModifierKeys.Control | ModifierKeys.Shift), OnCommandPromptElevated,
				CanExecuteIsSingleSelectionFileType));
		}
		public ICommand RefreshSelected {
			get => GetCommand(new RelayUICommand("Refresh Selected", Images.RefreshSelected,
				new KeyGesture(Key.F5), OnRefreshSelected, CanExecuteHasSelection));
		}
		public ICommand DeleteRecycle {
			get => GetCommand(new RelayUICommand("Delete (to Recycle Bin)", Images.Recycle,
				new KeyGesture(Key.Delete), OnDeleteRecycle, CanExecuteDelete));
		}
		public ICommand DeletePermanently {
			get => GetCommand(new RelayUICommand("Delete (Permanently!)", Images.Delete,
				new KeyGesture(Key.Delete, ModifierKeys.Shift), OnDeletePermanently, CanExecuteDelete));
		}
		public ICommand Properties {
			get => GetCommand(new RelayUICommand("Properties", Images.Properties,
				OnProperties, CanExecuteIsSingleSelectionFileType));
		}

		#endregion

		#region Other

		public ICommand EmptyRecycleBin {
			get => GetCommand(new RelayUICommand("Empty Recycle Bin", Images.DeleteRecycleBin,
				OnEmptyRecycleBin, CanExecuteEmptyRecycleBin));
		}

		public ICommand CancelScan {
			get => GetCommand(new RelayCommand(OnCancelScan, CanExecuteCancelScan));
		}

		public ICommand FileSort {
			get => GetCommand(new RelayCommand(OnFileSort));
		}

		public ICommand ExtensionSort {
			get => GetCommand(new RelayCommand(OnExtensionSort));
		}

		#endregion

		private bool CanExecuteIsFinished(object parameter) {
			return IsFinished;
		}
		private bool CanExecuteHasSelection(object parameter) {
			return selectedFiles.Count > 0;
		}
		private bool CanExecuteIsSingleSelection(object parameter) {
			return selectedFiles.Count == 1;
		}

		private bool CanExecuteIsSelectionFileType(object parameter) {
			return selectedFiles.Count > 0 && !selectedFiles.Any(f => !f.Model.IsFileType);
		}
		private bool CanExecuteIsSingleSelectionFileType(object parameter) {
			return selectedFiles.Count == 1 && selectedFiles[0].Model.IsFileType;
		}

		private bool CanExecuteExpand(object parameter) {
			return selectedFiles.Count == 1 && selectedFiles[0].ShowExpander;
		}
		private bool CanExecuteOpenItem(object parameter) {
			return selectedFiles.Count == 1 && selectedFiles[0].Model.IsFileType;
		}
		private bool CanExecuteExplore(object parameter) {
			return selectedFiles.Count == 1;
		}
		private bool CanExecuteDelete(object parameter) {
			return selectedFiles.Count > 0 && !selectedFiles.Any(f => !f.Model.IsFileType || f.Model.IsAnyRootType);
		}
		private bool CanExecuteRefreshSelected(object parameter) {
			return selectedFiles.Count > 0 && !selectedFiles.Any(f => !f.Model.IsFileType);
		}
		private bool CanExecuteEmptyRecycleBin(object parameter) {
			return false;
		}
		private bool CanExecuteCancelScan(object parameter) {
			return IsScanning;
		}


		#region File Menu

		private void OnOpen(object parameter) {
			DriveSelectEventArgs e = new DriveSelectEventArgs(settings.DriveSelectMode, settings.SelectedDrives, settings.SelectedFolderPath);
			DriveSelect?.Invoke(this, e);
			if (e.Result) {
				settings.DriveSelectMode = e.Mode;
				settings.SelectedDrives = e.SelectedDrives;
				settings.SelectedFolderPath = e.FolderPath;
				model.CancelAsync(() => model.ScanAsync(settings.ScanPriority, e.ResultPaths));
			}
		}
		private void OnSave(object parameter) {

		}
		private void OnReload(object parameter) {

		}
		private void OnClose(object parameter) {

		}
		private void OnElevate(object parameter) {

		}
		private void OnExit(object parameter) {
			Application.Current.MainWindow.Close();
		}

		#endregion

		#region Context Menu/Toolbar

		private void OnExpand(object parameter) {
			SelectedFile.IsExpanded = !SelectedFile.IsExpanded;
		}
		private void OnOpenItem(object parameter) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = SelectedFile.FullName,
			};
			Process.Start(startInfo)?.Dispose();
		}
		private void OnCopyPath(object parameter) {
			Clipboard.SetText($"\"{SelectedFile.FullName}\"");
		}
		private void OnExplore(object parameter) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = "explorer.exe",
				Arguments = $"/select,\"{SelectedFile.FullName}\"",
			};
			Process.Start(startInfo)?.Dispose();
		}
		private void OnCommandPrompt(object parameter) {
			string path = SelectedFile.FullName;
			if (SelectedFile.Type == FileNodeType.File)
				path = SelectedFile.Parent.FullName;
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = "cmd.exe",
				WorkingDirectory = path,
			};
			Process.Start(startInfo)?.Dispose();
		}
		private void OnCommandPromptElevated(object parameter) {
			string path = SelectedFile.FullName;
			if (SelectedFile.Type == FileNodeType.File)
				path = SelectedFile.Parent.FullName;
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = "cmd.exe",
				WorkingDirectory = path,
				Verb = "runas",
			};
			Process.Start(startInfo)?.Dispose();
		}
		private void OnPowerShell(object parameter) {

		}
		private void OnPowerShellElevated(object parameter) {

		}
		private void OnRefreshSelected(object parameter) {

		}
		private void OnDeleteRecycle(object parameter) {

		}
		private void OnDeletePermanently(object parameter) {

		}
		private void OnProperties(object parameter) {
			/*ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = SelectedFile.FullName,
				Verb = "properties",
				UseShellExecute = true,
			};
			Process.Start(startInfo).Dispose();*/
		}

		#endregion

		#region Other

		private void OnEmptyRecycleBin(object parameter) {

		}

		private void OnCancelScan(object parameter) {
			model.Cancel();
		}

		private void OnFileSort(object parameter) {
			rootNode?.Sort(fileComparer.Compare);
		}

		private void OnExtensionSort(object parameter) {
			extensions.Sort(extensionComparer.Compare);
		}

		#endregion
	}
}
