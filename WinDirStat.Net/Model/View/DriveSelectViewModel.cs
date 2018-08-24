using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WinDirStat.Net.Model.Settings;
using WinDirStat.Net.Model.View.Comparers;
using WinDirStat.Net.Model.View.Drives;
using WinDirStat.Net.SortingView;
using WinDirStat.Net.Utils;
using WinDirStat.Net.Windows;

namespace WinDirStat.Net.Model.View {
	public enum DriveSelectMode {
		All,
		Individual,
		Folder,
	}
	public partial class DriveSelectViewModel : ViewModel {

		private DriveSelectMode mode;
		private readonly ObservableCollection<DriveViewModel> selectedDrives;
		private string folderPath;
		private bool validSelection;
		private readonly DriveComparer driveComparer;
		private readonly DriveCollection drives;

		private IconCache icons;
		private WinDirStatSettings settings;

		public DriveSelectViewModel() {
			mode = DriveSelectMode.Individual;
			selectedDrives = new ObservableCollection<DriveViewModel>();
			folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			validSelection = false;
			driveComparer = new DriveComparer();
			icons = null;
			drives = new DriveCollection(this);
			selectedDrives.CollectionChanged += OnSelectionChanged;
		}

		private void OnSelectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			ValidateSelection();
		}

		public void Initialize(WinDirStatViewModel viewModel, DriveSelectEventArgs e) {
			Icons = viewModel.Icons;
			Settings = viewModel.Settings;
			Mode = e.Mode;
			SelectedDriveNames = e.SelectedDrives;
			FolderPath = e.FolderPath;
			drives.Refresh(driveComparer.Compare);
		}

		public IconCache Icons {
			get => icons;
			private set {
				if (icons != value) {
					icons = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public DriveCollection Drives {
			get => drives;
		}

		public WinDirStatSettings Settings {
			get => settings;
			private set {
				if (settings != value) {
					settings = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public DriveSelectMode Mode {
			get => mode;
			set {
				if (mode != value) {
					mode = value;
					ValidateSelection();
					AutoRaisePropertyChanged();
				}
			}
		}

		public ObservableCollection<DriveViewModel> SelectedDrives {
			get => selectedDrives;
		}

		public string[] DriveNames {
			get => drives.Select(d => d.Name).ToArray();
		}
		public string[] SelectedDriveNames {
			get => selectedDrives.Select(d => d.Name).ToArray();
			set {
				selectedDrives.Clear();
				foreach (DriveViewModel drive in drives) {
					if (value.Any(name => PathUtils.IsSamePath(name, drive.Name)))
						selectedDrives.Add(drive);
				}
			}
		}

		public string FolderPath {
			get => folderPath;
			set {
				if (folderPath != value) {
					folderPath = value;
					ValidateSelection();
					AutoRaisePropertyChanged();
				}
			}
		}

		public DriveComparer DriveComparer {
			get => driveComparer;
		}

		public bool IsValidSelection {
			get => validSelection;
			private set {
				if (validSelection != value) {
					validSelection = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public void Loaded() {
		}

		private bool ValidateSelection() {
			switch (mode) {
			case DriveSelectMode.All:
				IsValidSelection = true;
				break;
			case DriveSelectMode.Individual:
				IsValidSelection = selectedDrives.Count > 0;
				break;
			case DriveSelectMode.Folder:
				try {
					string fullPath = Path.GetFullPath(folderPath);
					IsValidSelection = Directory.Exists(folderPath);
				}
				catch {
					IsValidSelection = false;
				}
				break;
			}
			return IsValidSelection;
		}

		public ICommand DriveSort {
			get => GetCommand(new RelayCommand(OnDriveSort));
		}

		private void OnDriveSort(object parameter) {
			//driveComparer.SetSort(e);
			drives.Sort(driveComparer.Compare);
		}

		public void DriveListGotFocus() {
			Mode = DriveSelectMode.Individual;
		}
	}
}
