using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Data;
using WinDirStat.Net.Settings;

namespace WinDirStat.Net.Windows {
	public enum SelectDriveMode {
		All,
		Individual,
		Folder,
	}
	public class SelectDrivesDocument : IDisposable, INotifyPropertyChanged {


		private readonly WinDirDocument document;
		private readonly SelectDrivesList driveList;
		private readonly SelectDriveComparer driveComparer;
		private SelectDriveMode selectMode;
		private SelectDrive[] selectedDrives;
		private string folderPath;
		private bool validSelection;

		public SelectDrivesDocument(WinDirDocument document) {
			this.document = document;
			driveList = new SelectDrivesList(this);
			driveComparer = new SelectDriveComparer();
			selectMode = SelectDriveMode.All;
			validSelection = true;
			selectedDrives = new SelectDrive[0];
			folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		}

		public WinDirDocument Document {
			get => document;
		}

		public WinDirSettings Settings {
			get => document.Settings;
		}

		public SelectDrivesList Drives {
			get => driveList;
		}

		public IconCache Icons {
			get => document.Icons;
		}

		public SelectDriveComparer DriveComparer {
			get => driveComparer;
		}

		public string FolderPath {
			get => folderPath;
			set {
				if (folderPath != value) {
					folderPath = value;
					AutoRaisePropertyChanged();
					InvalidateSelection();
				}
			}
		}

		public ReadOnlyCollection<SelectDrive> SelectedDrives {
			get => Array.AsReadOnly(selectedDrives);
		}

		public void SetSelectedDrives(IEnumerable<SelectDrive> drives) {
			selectedDrives = drives.ToArray();
			InvalidateSelection();
		}

		private bool InvalidateSelection() {
			bool newValidSelection = true;
			switch (selectMode) {
			case SelectDriveMode.Individual:
				newValidSelection = selectedDrives.Length > 0;
				break;
			case SelectDriveMode.Folder:
				try {
					string fullPath = Path.GetFullPath(folderPath);
					newValidSelection = Directory.Exists(folderPath);
				}
				catch {
					newValidSelection = false;
				}
				break;
			}
			if (validSelection != newValidSelection) {
				validSelection = newValidSelection;
				RaisePropertyChanged(nameof(ValidSelection));
			}
			return newValidSelection;
		}

		public bool ValidSelection {
			get => validSelection;
		}

		public SelectDriveMode SelectMode {
			get => selectMode;
			set {
				if (selectMode != value) {
					selectMode = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public void SetDriveSort(SelectDriveSortMethod method, ListSortDirection direction) {
			bool methodChanged = driveComparer.SortMethod != method;
			bool directionChanged = driveComparer.SortDirection != direction;
			driveComparer.SortMethod = method;
			driveComparer.SortDirection = direction;
			if (methodChanged || directionChanged)
				driveList.Sort();
		}


		public void Dispose() {
			
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
		private void AutoRaisePropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
