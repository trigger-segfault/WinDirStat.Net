using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.ViewModel {
    partial class MainViewModel {
		
		#region File Menu
		
		public IRelayCommand Open => GetCommand(OnOpen);
		public IRelayCommand Save => GetCommand(OnSave, CanExecuteIsOpen);
		public IRelayCommand Reload => GetCommand(OnReload, CanExecuteIsOpen);
		public IRelayCommand Close => GetCommand(OnClose, CanExecuteIsOpen);
		public IRelayCommand Cancel => GetCommand(OnCancel, CanExecuteIsScanning);
		public IRelayCommand Elevate => GetCommand(OnElevate, CanExecuteElevate);
		public IRelayCommand Exit => GetCommand(OnExit);

		#endregion

		#region Context Menu/Toolbar

		public IRelayCommand Expand => GetCommand(OnExpand, CanExecuteExpand);
		public IRelayCommand Collapse => GetCommand(OnExpand, CanExecuteExpand);

		public IRelayCommand OpenItem => GetCommand(OnOpenItem, CanExecuteOpenItem);
		public IRelayCommand CopyPath => GetCommand(OnCopyPath, CanExecuteCopyPath);
		public IRelayCommand Explore => GetCommand(OnExplore, CanExecuteExplore);
		public IRelayCommand CommandPrompt => GetCommand(OnCommandPrompt, CanExecuteCommandPrompt);
		public IRelayCommand PowerShell => GetCommand(OnPowerShell, CanExecuteCommandPrompt);
		public IRelayCommand RefreshSelected => GetCommand(OnRefreshSelected, CanExecuteRefreshSelected);
		public IRelayCommand DeleteRecycle => GetCommand(OnDeleteRecycle, CanExecuteDeleteSingle);
		public IRelayCommand DeletePermanently => GetCommand(OnDeletePermanently, CanExecuteDeleteSingle);
		public IRelayCommand Properties => GetCommand(OnProperties, CanExecuteOpenItem);

		#endregion

		#region Options Menu

		public IRelayCommand ShowFreeSpace => GetCommand(OnShowFreeSpace);
		public IRelayCommand ShowUnknown => GetCommand(OnShowUnknown);
		public IRelayCommand ShowTotalSpace => GetCommand(OnShowTotalSpace);
		public IRelayCommand ShowFileTypes => GetCommand(OnShowFileTypes);
		public IRelayCommand ShowTreemap => GetCommand(OnShowTreemap);
		public IRelayCommand ShowToolBar => GetCommand(OnShowToolBar);
		public IRelayCommand ShowStatusBar => GetCommand(OnShowStatusBar);
		public IRelayCommand Configure => GetCommand(OnConfigure);

		#endregion

		#region Other

		public IRelayCommand EmptyRecycleBin => GetCommand(OnEmptyRecycleBin, CanExecuteEmptyRecycleBin);

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
		private async void OnElevate() {
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
					await DisposeAsync();
				}
				catch {
					Dialogs.ShowError(WindowOwner, "Failed to start elevated process!", "Error");
				}
			}
		}
		private async void OnExit() {
			UI.Shutdown();
			await DisposeAsync();
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
			if (OS.EmptyRecycleBin(WindowOwner)) {
				UpdateEmptyRecycleBin(true);
			}
		}

		private void OnConfigure() {
			// Open settings window
			//Dialogs.ShowSettings(WindowOwner);
		}

		#endregion
	}
}
