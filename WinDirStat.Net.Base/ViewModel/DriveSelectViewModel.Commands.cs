using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Drives;

#if WPF
using GalaSoft.MvvmLight.CommandWpf;
#else
using GalaSoft.MvvmLight.Command;
#endif

namespace WinDirStat.Net.ViewModel {
	partial class DriveSelectViewModel {
		
		public RelayCommand OK {
			get => GetCommand(OnOK);
		}

		public RelayCommand SelectFolder {
			get => GetCommand(OnSelectFolder);
		}

		private void OnOK() {
			// Apply the settings for future use
			Settings.DriveSelectMode = mode;
			Settings.SelectedDrives = SelectedDrives.Select(d => d.Name).ToArray();
			Settings.SelectedFolderPath = folderPath;
			Result = new DriveSelectResult(Scanning,
										   Settings.DriveSelectMode,
										   Settings.SelectedDrives,
										   Settings.SelectedFolderPath);
		}

		private void OnSelectFolder() {
			string newFolder = Dialogs.ShowFolderBrowser(WindowOwner, "WinDirStat.Net - Select Folder", false);
			if (newFolder != null) {
				FolderPath = newFolder;
			}
		}

	}
}
