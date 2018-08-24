using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Windows;

namespace WinDirStat.Net.Model.View {
	public class DriveSelectEventArgs {
		// In/Out
		public DriveSelectMode Mode { get; set; }
		public string[] SelectedDrives { get; set; }
		public string FolderPath { get; set; }

		// Out
		public bool Result { get; set; }
		public string[] ResultPaths { get; set; }

		public DriveSelectEventArgs(DriveSelectMode mode, string[] selectedDrives, string folderPath) {
			Mode = mode;
			SelectedDrives = selectedDrives;
			FolderPath = folderPath;

			Result = false;
			ResultPaths = new string[0];
		}
	}

	partial class WinDirStatViewModel {

		public event EventHandler<DriveSelectEventArgs> DriveSelect;
	}
}
