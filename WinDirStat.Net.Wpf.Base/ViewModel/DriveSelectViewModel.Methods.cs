using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.ViewModel.Drives;

namespace WinDirStat.Net.ViewModel {
	partial class DriveSelectViewModel {

		public void Loaded() {
			SelectedDrives.Clear();
			Drives.Model.Refresh();
			Mode = Settings.DriveSelectMode;
			FolderPath = Settings.SelectedFolderPath;
			var drivesToSelect = Drives.Where(d => Settings.SelectedDrives.Any(n => d.Name == n));
			foreach (DriveItemViewModel drive in drivesToSelect)
				SelectedDrives.Add(drive);
		}

		public void SortDrives() {
			Drives.Sort(DriveComparer.Compare);
		}

		public void ListGotFocus() {
			Mode = DriveSelectMode.Individual;
		}

	}
}
