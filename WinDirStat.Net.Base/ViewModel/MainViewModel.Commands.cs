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
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.ViewModel {
	partial class MainViewModel {
		
		#region File Menu
		
		public IRelayUICommand Open {
			get => GetCommand("Open...", Images.Open, Shortcuts.Open,
				OnOpen);
		}
		public IRelayUICommand Save {
			get => GetCommand("Save...", Images.Save, Shortcuts.Save,
				OnSave, CanExecuteIsOpen);
		}
		public IRelayUICommand Reload {
			get => GetCommand("Reload", Images.Reload, Shortcuts.Reload,
				OnReload, CanExecuteIsOpen);
		}
		public IRelayUICommand Close {
			get => GetCommand("Close", Images.Close, Shortcuts.Close,
				OnClose, CanExecuteIsOpen);
		}
		public IRelayCommand Cancel {
			get => GetCommand("Cancel", (IImage) null, Shortcuts.Cancel,
				OnCancel, CanExecuteIsScanning);
		}
		public IRelayUICommand Elevate {
			get => GetCommand("Elevate", Images.Elevate, Shortcuts.Elevate,
				OnElevate, CanExecuteElevate);
		}
		public IRelayUICommand Exit {
			get => GetCommand("Exit", Images.Exit, Shortcuts.Exit,
				OnExit);
		}

		#endregion

		#region Context Menu/Toolbar

		public IRelayUICommand Expand {
			get => GetCommand("Expand", Images.Expand, Shortcuts.Expand,
				OnExpand, CanExecuteExpand);
		}
		public IRelayUICommand Collapse {
			get => GetCommand("Collapse", Images.Expand, Shortcuts.Expand,
				OnExpand, CanExecuteExpand);
		}

		public IRelayUICommand OpenItem {
			get => GetCommand("Open Item", Images.Run, Shortcuts.OpenItem,
				OnOpenItem, CanExecuteOpenItem);
		}
		public IRelayUICommand CopyPath {
			get => GetCommand("Copy Path", Images.CopyPath, Shortcuts.CopyPath,
				OnCopyPath, CanExecuteIsSelectionSingleFileType);
		}
		public IRelayUICommand Explore {
			get => GetCommand("Explore Here", Images.Explore, Shortcuts.Explore,
				OnExplore, CanExecuteExplore);
		}
		public IRelayUICommand CommandPrompt {
			get => GetCommand("Command Prompt Here", Images.Cmd, Shortcuts.CommandPrompt,
				OnCommandPrompt, CanExecuteCommandPrompt);
		}
		public IRelayUICommand PowerShell {
			get => GetCommand("PowerShell Here", Images.PowerShell, Shortcuts.PowerShell,
				OnPowerShell, CanExecuteIsSelectionSingleFileType);
		}
		public IRelayUICommand RefreshSelected {
			get => GetCommand("Refresh Selected", Images.RefreshSelected, Shortcuts.RefreshSelected,
				OnRefreshSelected, CanExecuteRefreshSelected);
		}
		public IRelayUICommand DeleteRecycle {
			get => GetCommand("Delete (to Recycle Bin)", Images.RecycleBin, Shortcuts.DeleteRecycle,
				OnDeleteRecycle, CanExecuteDeleteSingle);
		}
		public IRelayUICommand DeletePermanently {
			get => GetCommand("Delete (Permanently!)", Images.Delete, Shortcuts.DeletePermanently,
				OnDeletePermanently, CanExecuteDeleteSingle);
		}
		public IRelayUICommand Properties {
			get => GetCommand("Properties", Images.Properties, Shortcuts.Properties,
				OnProperties, CanExecuteOpenItem);
		}

		#endregion

		#region Options Menu

		public IRelayUICommand ShowFreeSpace {
			get => GetCommand("Show Free Space", Images.FreeSpace, Shortcuts.Properties,
				OnShowFreeSpace);
		}
		public IRelayUICommand ShowUnknown {
			get => GetCommand("Show Unknown", Images.UnknownSpace, Shortcuts.Properties,
				OnShowUnknown);
		}
		public IRelayUICommand ShowTotalSpace {
			get => GetCommand("Show Total Space", Images.ShowTotalSpace, Shortcuts.Properties,
				OnShowTotalSpace);
		}
		public IRelayUICommand ShowFileTypes {
			get => GetCommand("Show File Types", Images.FileCollection, Shortcuts.Properties,
				OnShowFileTypes);
		}
		public IRelayUICommand ShowTreemap {
			get => GetCommand("Show Treemap", Images.ShowTreemap, Shortcuts.Properties,
				OnShowTreemap);
		}
		public IRelayUICommand ShowToolBar {
			get => GetCommand("Show Toolbar", (IImage) null, Shortcuts.Properties,
				OnShowToolBar);
		}
		public IRelayUICommand ShowStatusBar {
			get => GetCommand("Show Statusbar", (IImage) null, Shortcuts.Properties,
				OnShowStatusBar);
		}
		public IRelayUICommand Configure {
			get => GetCommand("Configure WinDirStat...", Images.Settings,
				OnConfigure);
		}

		#endregion

		#region Other

		public IRelayUICommand EmptyRecycleBin {
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
			MessageResult result = MessageResult.Yes;
			if (Scanning.ScanState != ScanState.NotStarted) {
				result = Dialogs.ShowWarning(WindowOwner,
					"Elevating the process will start a new instance and all progress will be lost. Would you like to continue?",
					"Elevate", MessageButton.YesNo);
			}
			if (result == MessageResult.Yes) {
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
