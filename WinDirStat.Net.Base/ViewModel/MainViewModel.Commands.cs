using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
#if WPF
using GalaSoft.MvvmLight.CommandWpf;
#else
using GalaSoft.MvvmLight.Command;
#endif
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.ViewModel {
	partial class MainViewModel {
		
		#region File Menu
		
		public RelayUICommand Open {
			get => GetCommand("Open...", Images.Open, new KeyGesture(Key.O, ModifierKeys.Control),
				OnOpen);
		}
		public RelayUICommand Save {
			get => GetCommand("Save...", Images.Save, new KeyGesture(Key.S, ModifierKeys.Control),
				OnSave, CanExecuteIsOpen);
		}
		public RelayUICommand Reload {
			get => GetCommand("Reload", Images.Reload, new KeyGesture(Key.F5, ModifierKeys.Shift),
				OnReload, CanExecuteIsOpen);
		}
		public RelayUICommand Close {
			get => GetCommand("Close", Images.Close,
				OnClose, CanExecuteIsOpen);
		}
		public RelayCommand Cancel {
			get => GetCommand("Cancel",
				OnCancel, CanExecuteIsScanning);
		}
		public RelayUICommand Elevate {
			get => GetCommand("Elevate", Images.Elevate,
				OnElevate, CanExecuteElevate);
		}
		public RelayUICommand Exit {
			get => GetCommand("Exit", Images.Exit, new KeyGesture(Key.W, ModifierKeys.Control),
				OnExit);
		}

		#endregion

		#region Context Menu/Toolbar

		public RelayUICommand Expand {
			get => GetCommand("Expand", Images.Expand,
				OnExpand, CanExecuteExpand);
		}
		public RelayUICommand Collapse {
			get => GetCommand("Collapse", Images.Expand,
				OnExpand, CanExecuteExpand);
		}

		public RelayUICommand OpenItem {
			get => GetCommand("Open Item", Images.Run, new KeyGesture(Key.Return),
				OnOpenItem, CanExecuteOpenItem);
		}
		public RelayUICommand CopyPath {
			get => GetCommand("Copy Path", Images.CopyPath, new KeyGesture(Key.C, ModifierKeys.Control),
				OnCopyPath, CanExecuteIsSelectionSingleFileType);
		}
		public RelayUICommand Explore {
			get => GetCommand("Explore Here", Images.Explore, new KeyGesture(Key.C, ModifierKeys.Control),
				OnExplore, CanExecuteExplore);
		}
		public RelayUICommand CommandPrompt {
			get => GetCommand("Command Prompt Here", Images.Cmd, new KeyGesture(Key.P, ModifierKeys.Control),
				OnCommandPrompt, CanExecuteCommandPrompt);
		}
		public RelayUICommand PowerShell {
			get => GetCommand("PowerShell Here", Images.PowerShell, new KeyGesture(Key.P, ModifierKeys.Control | ModifierKeys.Shift),
				OnPowerShell, CanExecuteIsSelectionSingleFileType);
		}
		public RelayUICommand RefreshSelected {
			get => GetCommand("Refresh Selected", Images.RefreshSelected, new KeyGesture(Key.F5),
				OnRefreshSelected, CanExecuteRefreshSelected);
		}
		public RelayUICommand DeleteRecycle {
			get => GetCommand("Delete (to Recycle Bin)", Images.RecycleBin, new KeyGesture(Key.Delete),
				OnDeleteRecycle, CanExecuteDeleteSingle);
		}
		public RelayUICommand DeletePermanently {
			get => GetCommand("Delete (Permanently!)", Images.Delete, new KeyGesture(Key.Delete, ModifierKeys.Shift),
				OnDeletePermanently, CanExecuteDeleteSingle);
		}
		public RelayUICommand Properties {
			get => GetCommand("Properties", Images.Properties,
				OnProperties, CanExecuteOpenItem);
		}

		#endregion

		#region Options Menu

		public RelayUICommand ShowFreeSpace {
			get => GetCommand("Show Free Space", Images.FreeSpace, new KeyGesture(Key.F6),
				OnShowFreeSpace);
		}
		public RelayUICommand ShowUnknown {
			get => GetCommand("Show Unknown", Images.UnknownSpace, new KeyGesture(Key.F7),
				OnShowUnknown);
		}
		public RelayUICommand ShowTotalSpace {
			get => GetCommand("Show Total Space", Images.ShowTotalSpace,
				OnShowTotalSpace);
		}
		public RelayUICommand ShowFileTypes {
			get => GetCommand("Show File Types", Images.FileCollection, new KeyGesture(Key.F8),
				OnShowFileTypes);
		}
		public RelayUICommand ShowTreemap {
			get => GetCommand("Show Treemap", Images.ShowTreemap, new KeyGesture(Key.F9),
				OnShowTreemap);
		}
		public RelayUICommand ShowToolBar {
			get => GetCommand("Show Toolbar", (ImageSource) null, OnShowToolBar);
		}
		public RelayUICommand ShowStatusBar {
			get => GetCommand("Show Statusbar", (ImageSource) null, OnShowStatusBar);
		}
		public RelayUICommand Configure {
			get => GetCommand("Configure WinDirStat...", Images.Settings,
				OnConfigure);
		}

		#endregion

		#region Other

		public RelayUICommand EmptyRecycleBin {
			get => GetCommand("Empty Recycle Bins", Images.EmptyRecycleBin,
				OnEmptyRecycleBin, CanExecuteEmptyRecycleBin);
		}

		#endregion


		#region CanExecute

		private bool CanExecuteIsOpen() {
			return IsOpen;
		}
		private bool CanExecuteIsScanning() {
			return IsScanning;
		}

		private bool CanExecuteHasSelection() {
			return SelectedFiles.Count > 0;
		}
		private bool CanExecuteIsSingleSelection() {
			return SelectedFiles.Count == 1;
		}

		private bool CanExecuteIsSelectionFileType() {
			return SelectedFiles.Count > 0 && !SelectedFiles.Any(f => !f.Model.IsFileType);
		}
		private bool CanExecuteIsSelectionSingleFileType() {
			return SelectedFiles.Count == 1 && SelectedFiles[0].Model.IsFileType;
		}
		private bool CanExecuteCommandPrompt() {
			return CanExecuteIsSelectionSingleFileType();
		}
		private bool CanExecuteCopyPath() {
			return CanExecuteIsSelectionSingleFileType();
		}

		private bool CanExecuteExpand() {
			return SelectedFiles.Count == 1;
		}
		private bool CanExecuteOpenItem() {
			return SelectedFiles.Count == 1 && (SelectedFile.Model.IsFileType ||
												SelectedFile.Model.IsAbsoluteRootType);
		}
		private bool CanExecuteExplore() {
			return SelectedFiles.Count == 1 && (SelectedFile.Model.IsFileType ||
												SelectedFile.Model.Type == FileItemType.FileCollection ||
												SelectedFile.Model.IsAbsoluteRootType);
		}
		private bool CanExecuteDelete() {
			return SelectedFiles.Count > 0 && !SelectedFiles.Any(f => !f.Model.IsFileType || f.Model.IsAnyRootType);
		}
		private bool CanExecuteDeleteSingle() {
			return SelectedFiles.Count == 1 && SelectedFile.Model.IsFileType && !SelectedFile.Model.IsAnyRootType;
		}
		private bool CanExecuteRefreshSelected() {
			return IsOpen && SelectedFiles.Count > 0 && !SelectedFiles.Any(f => !f.Model.IsFileType && !f.Model.IsContainerType);
		}
		private bool CanExecuteEmptyRecycleBin() {
			return allRecycleBinInfo.ItemCount != 0 && allRecycleBinInfo.Size != 0;
		}
		private bool CanExecuteElevate() {
			return !OS.IsElevated;
		}

		#endregion

		#region File Menu

		private void OnOpen() {
			DriveSelectResult result = Dialogs.ShowDriveSelect(WindowOwner);
			if (result != null) {
				Scanning.CloseAsync(() => Scanning.ScanAsync(result));
			}
		}
		private void OnSave() {
			// TODO: Implement saving scanned file tree
		}
		private void OnReload() {
			Scanning.ReloadAsync();
		}
		private void OnClose() {
			Scanning.Close(false);
		}
		private void OnCancel() {
			Scanning.Cancel(false);
		}
		private void OnElevate() {
			// Dialog warn progress will be lost.
			// If yes, close then start new process.
			MessageBoxResult result = MessageBoxResult.Yes;
			if (Scanning.ScanState != ScanState.NotStarted) {
				result = Dialogs.ShowWarning(WindowOwner,
					"Elevating the process will start a new instance and all progress will be lost. Would you like to continue?",
					"Elevate", MessageBoxButton.YesNo);
			}
			if (result == MessageBoxResult.Yes) {
				try {
					OS.StartNewElevated();
					UI.Shutdown();
					Dispose();
				}
				catch {
					Dialogs.ShowError(WindowOwner, "Failed to start elevated process!", "Error");
				}
			}
		}
		private void OnExit() {
			UI.Shutdown();
			Dispose();
		}

		#endregion

		#region Context Menu/Toolbar

		private void OnExpand() {
			SelectedFile.IsExpanded = !SelectedFile.IsExpanded;
		}
		private void OnOpenItem() {
			if (SelectedFile.Type == FileItemType.Computer)
				OS.ExploreComputer();
			OS.RunItem(SelectedFile.FullName);
		}
		private void OnCopyPath() {
			Clipboard.SetText($"\"{SelectedFile.FullName}\"");
		}
		private void OnExplore() {
			switch (SelectedFile.Type) {
			case FileItemType.Computer:
				OS.ExploreComputer();
				break;
			case FileItemType.File:
				OS.ExploreFile(SelectedFile.FullName);
				break;
			default:
				OS.ExploreFolder(SelectedFile.FullName);
				break;
			}
		}
		private void OnCommandPrompt() {
			string directory = SelectedFile.FullName;
			if (SelectedFile.Type == FileItemType.File)
				directory = SelectedFile.Parent.FullName;
			OS.OpenCommandPrompt(directory);
		}
		private void OnPowerShell() {
			string directory = SelectedFile.FullName;
			if (SelectedFile.Type == FileItemType.File)
				directory = SelectedFile.Parent.FullName;
			OS.OpenPowerShell(directory);
		}
		private void OnRefreshSelected() {
			HashSet<FileItemBase> filesToRefresh = new HashSet<FileItemBase>();
			IsolateRefreshableFiles(filesToRefresh, SelectedFiles.Select(f => f.Model));
			if (filesToRefresh.Count > 0)
				Scanning.RefreshFilesAsync(filesToRefresh);
		}
		private void OnDeleteRecycle() {
			Scanning.RecycleFile(SelectedFile.Model);
		}
		private void OnDeletePermanently() {
			Scanning.DeleteFile(SelectedFile.Model);
		}
		private void OnProperties() {
			OS.OpenProperties(SelectedFile.FullName);
		}

		private void IsolateRefreshableFiles(HashSet<FileItemBase> filesToRefresh, IEnumerable<FileItemBase> files) {
			foreach (FileItemBase file in files) {
				if (file.IsFileType) {
					filesToRefresh.Add(file);
				}
				else if (file.IsContainerType) {
					IsolateRefreshableFiles(filesToRefresh, file.Children);
				}
			}
		}

		#endregion

		#region Options

		private void OnShowFreeSpace() {
			Settings.ShowFreeSpace = !Settings.ShowFreeSpace;
		}
		private void OnShowUnknown() {
			Settings.ShowUnknown = !Settings.ShowUnknown;
		}
		private void OnShowTotalSpace() {
			Settings.ShowTotalSpace = !Settings.ShowTotalSpace;
		}
		private void OnShowFileTypes() {
			Settings.ShowFileTypes = !Settings.ShowFileTypes;
		}
		private void OnShowTreemap() {
			Settings.ShowTreemap = !Settings.ShowTreemap;
		}
		private void OnShowToolBar() {
			Settings.ShowToolBar = !Settings.ShowToolBar;
		}
		private void OnShowStatusBar() {
			Settings.ShowStatusBar = !Settings.ShowStatusBar;
		}

		#endregion

		#region Other

		private void OnEmptyRecycleBin() {
			OS.EmptyRecycleBin(WindowOwner);
		}

		private void OnConfigure() {
			// Open settings window
			//Dialogs.ShowSettings(WindowOwner);
		}

		#endregion
	}
}
