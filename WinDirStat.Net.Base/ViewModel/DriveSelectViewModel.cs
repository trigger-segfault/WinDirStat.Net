using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Services;
using WinDirStat.Net.ViewModel;
using WinDirStat.Net.ViewModel.Comparers;
using WinDirStat.Net.ViewModel.Drives;

namespace WinDirStat.Net.ViewModel {
	/// <summary>The view model for the drive select dialog.</summary>
	public partial class DriveSelectViewModel : ViewModelWindowBase {

		#region Fields

		// Services
		/// <summary>Gets the program settings service.</summary>
		public SettingsService Settings { get; }
		/// <summary>Gets the UI service.</summary>
		public IUIService UI { get; }
		/// <summary>Gets the dialog service.</summary>
		public IWindowDialogService Dialogs { get; }
		/// <summary>Gets the icon cache service.</summary>
		public IIconCacheService IconCache { get; }
		/// <summary>Gets the scanning service.</summary>
		public ScanningService Scanning { get; }

		// Collections
		/// <summary>Gets the drive collection.</summary>
		public DriveItemViewModelCollection Drives { get; }
		/// <summary>Gets the comparer for the drives.</summary>
		public DriveComparer DriveComparer { get; }

		// Settings
		/// <summary>The root path selection mode.</summary>
		private DriveSelectMode mode;
		/// <summary>Gets the selected individual drives.</summary>
		public ObservableCollection<DriveItemViewModel> SelectedDrives { get; }
		/// <summary>The current folder path.</summary>
		private string folderPath;

		// State
		/// <summary>The result of the drive selection.</summary>
		private DriveSelectResult result;
		/// <summary>True if the current mode's selection is valid.</summary>
		private bool validSelection;
		
		#endregion

		#region Constructors

		/// <summary>Constructrs the <see cref="DriveSelectViewModel"/>.</summary>
		public DriveSelectViewModel(SettingsService settings,
									IUIService ui,
									IWindowDialogService dialog,
									IIconCacheService iconCache,
									ScanningService scanning)
		{
			Settings = settings;
			UI = ui;
			Dialogs = dialog;
			IconCache = iconCache;
			Scanning = scanning;

			Drives = new DriveItemViewModelCollection(this);
			DriveComparer = new DriveComparer();

			SelectedDrives = new ObservableCollection<DriveItemViewModel>();
			SelectedDrives.CollectionChanged += OnSelectedDrivesChanged;
		}

		#endregion

		#region Override Properties

		/// <summary>Gets the title to display for the window.</summary>
		public override string Title => "WinDirStat.Net - Drive Select";

		#endregion

		#region Properties

		/// <summary>Gets or sets the root path selection mode.</summary>
		public DriveSelectMode Mode {
			get => mode;
			set {
				if (mode != value) {
					mode = value;
					ValidateSelection();
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>Gets or sets the current folder path.</summary>
		public string FolderPath {
			get => folderPath;
			set {
				if (folderPath != value) {
					folderPath = value;
					if (mode == DriveSelectMode.Folder)
						ValidateSelection();
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>Gets or sets if the current selection is valid.</summary>
		public bool IsValidSelection {
			get => validSelection;
			private set {
				if (validSelection != value) {
					validSelection = value;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>Gets the result root paths.</summary>
		public DriveSelectResult Result {
			get => result;
			private set => Set(ref result, value);
		}

		#endregion

		#region Validation

		/// <summary>Validates the current mode's selection.</summary>
		private void ValidateSelection() {
			switch (mode) {
			case DriveSelectMode.All:
				IsValidSelection = true;
				break;
			case DriveSelectMode.Individual:
				IsValidSelection = SelectedDrives.Count > 0;
				break;
			case DriveSelectMode.Folder:
				try {
					IsValidSelection = Directory.Exists(Path.GetFullPath(folderPath));
				}
				catch {
					IsValidSelection = false;
				}
				break;
			}
		}

		#endregion

		#region Event Handlers
		
		/// <summary>Validates the selection when the drive selection changes.</summary>
		private void OnSelectedDrivesChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (mode == DriveSelectMode.Individual)
				ValidateSelection();
		}

		#endregion
	}
}
