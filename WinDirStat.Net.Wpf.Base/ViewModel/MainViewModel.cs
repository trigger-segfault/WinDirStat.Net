using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using WinDirStat.Net.Model.Files;
using WinDirStat.Net.Rendering;
using WinDirStat.Net.Services;
using WinDirStat.Net.Utils;
using WinDirStat.Net.ViewModel.Comparers;
using WinDirStat.Net.ViewModel.Extensions;
using WinDirStat.Net.ViewModel.Files;

namespace WinDirStat.Net.ViewModel {
	/// <summary>The main view model for the program.</summary>
	public partial class MainViewModel : ViewModelBaseEx {

		#region Fields

		public SettingsService Settings { get; }
		public ScanningService Scanning { get; }
		public IconCacheService IconCache { get; }
		public UIService UI { get; }
		public BitmapFactory BitmapFactory { get; }
		/// <summary>Gets the images service.</summary>
		public ImagesServiceBase Images { get; }
		public ClipboardService Clipboard { get; }
		public OSService OS { get; }
		public IMyDialogService Dialogs { get; }
		public TreemapRenderer Treemap { get; }

		public ExtensionItemViewModelCollection Extensions { get; }

		public ObservableCollection<FileItemViewModel> SelectedFiles { get; }
		public FileItemViewModel SelectedFile { get; private set; }
		private ExtensionItemViewModel selectedExtension;

		public FileComparer FileComparer { get; }
		public ExtensionComparer ExtensionComparer { get; }
		
		private long gcRAMUsage;
		private FileItemViewModel rootItem;
		private RootItem graphViewRootItem;
		private readonly UITimer ramTimer;
		private readonly UITimer statusTimer;

		private RecycleBinInfo allRecycleBinInfo;
		private string emptyRecycleBinLabel;

		private bool suppressRefresh;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel(SettingsService settings,
							 ScanningService scanning,
							 IconCacheService iconCache,
							 UIService ui,
							 BitmapFactory bitmapFactory,
							 ImagesServiceBase images,
							 ClipboardService clipboard,
							 OSService os,
							 IMyDialogService dialogs,
							 TreemapRendererFactory treemapFactory)
		{
			Settings = settings;
			Scanning = scanning;
			IconCache = iconCache;
			UI = ui;
			BitmapFactory = bitmapFactory;
			Images = images;
			Clipboard = clipboard;
			OS = os;
			Dialogs = dialogs;
			Treemap = treemapFactory.Create();

			Settings.PropertyChanged += OnSettingsPropertyChanged;
			Scanning.PropertyChanged += OnScanningPropertyChanged;

			Extensions = new ExtensionItemViewModelCollection(this);

			SelectedFiles = new ObservableCollection<FileItemViewModel>();
			SelectedFiles.CollectionChanged += OnSelectedFilesChanged;

			FileComparer = new FileComparer();
			ExtensionComparer = new ExtensionComparer();
			UpdateEmptyRecycleBin();
			
			GCRAMUsage = GC.GetTotalMemory(false);
			if (IsInDesignMode) {
				// Code runs in Blend --> create design time data.
			}
			else {
				// Code runs "for real"
				ramTimer = UI.StartTimer(Settings.RAMInterval, true, OnRAMUsageTick);
				statusTimer = UI.CreateTimer(Settings.StatusInterval, true, OnStatusTick);
			}
		}

		#endregion

		#region Properties

		/// <summary>Gets the title to display for the window.</summary>
		public override string Title {
			get {
				string title = "WinDirStat.Net";
				if (Scanning.ProgressState != ScanProgressState.NotStarted) {
					var paths = Scanning.RootPaths.Select(p => PathUtils.TrimSeparatorEnd(p));
					title = $"{string.Join("|", paths)} - {title}";
				}
				if (OS.IsElevated)
					title += " (Administrator)";
				return title;
			}
		}

		/// <summary>Gets the Garbage Collector's RAM Usage.</summary>
		public long GCRAMUsage {
			get => gcRAMUsage;
			set => Set(ref gcRAMUsage, value);
		}

		/// <summary>Gets the root file tree item.</summary>
		public FileItemViewModel RootItem {
			get => rootItem;
			private set {
				if (rootItem != value) {
					if (rootItem != null) {
						SuppressFileTreeRefresh = true;
						rootItem.IsExpanded = false;
						SuppressFileTreeRefresh = false;
						rootItem.Dispose();
					}
					rootItem = value;
					RaisePropertyChanged();
				}
			}
		}

		/// <summary>Gets treemap's root file tree item.</summary>
		public RootItem GraphViewRootItem {
			get => graphViewRootItem;
			private set => Set(ref graphViewRootItem, value);
		}

		#endregion

		#region ScanProperties

		/// <summary>Gets if a scan operation is in progress.</summary>
		public bool IsScanning => Scanning.IsScanning;
		/// <summary>Gets if a refresh operation is in progress.</summary>
		public bool IsRefreshing => Scanning.IsRefreshing;
		/// <summary>Gets if a scan is finished and open.</summary>
		public bool IsOpen => Scanning.IsOpen;
		/// <summary>Gets the scan time.</summary>
		public TimeSpan ScanTime => Scanning.ScanTime;
		/// <summary>Gets the scan progress.</summary>
		public double ScanProgress => Scanning.Progress;
		/// <summary>Gets if scan progress can be displayed.</summary>
		public bool CanDisplayScanProgress => Scanning.CanDisplayProgress;

		/// <summary>Gets or sets if the current scan operation is suspended.</summary>
		public bool IsScanSuspended {
			get => Scanning.IsSuspended;
			set => Scanning.IsSuspended = value;
		}
		
		///<summary>Gets if the file list has a selection.</summary>
		public bool HasFileSelection => SelectedFile != null;

		///<summary>Gets the file type list selection.</summary>
		public ExtensionItemViewModel SelectedExtension {
			get => selectedExtension;
			set {
				if (selectedExtension != value) {
					bool hasSelectionChanged = ((selectedExtension == null) != (value == null));
					selectedExtension = value;
					RaisePropertyChanged();
					if (hasSelectionChanged)
						RaisePropertyChanged(nameof(HasExtensionSelection));
				}
			}
		}

		///<summary>Gets if the file type list has a selection.</summary>
		public bool HasExtensionSelection => selectedExtension != null;

		/// <summary>Gets the label to display for the empty recycle bin command.</summary>
		public string EmptyRecycleBinLabel {
			get => emptyRecycleBinLabel;
			private set => Set(ref emptyRecycleBinLabel, value);
		}
		
		/// <summary>Gets if the UI refreshing should be supressed due to validation.</summary>
		public bool SuppressFileTreeRefresh {
			get => suppressRefresh || Scanning.SuppressFileTreeRefresh;
			private set {
				if (suppressRefresh != value) {
					suppressRefresh = value;
					if (!Scanning.SuppressFileTreeRefresh) {
						if (!suppressRefresh)
							RootItem?.RaiseChildrenReset();
						RaisePropertyChanged(nameof(SuppressFileTreeRefresh));
					}
				}
			}
		}

		#endregion

		#region Visibility Properties

		/// <summary>Gets if the file types list should be hidden.</summary>
		public bool HideFileTypes {
			get => (!Settings.ShowFileTypes || (!Scanning.IsRefreshing && !Scanning.IsOpen)) && !IsInDesignMode;
		}
		/// <summary>Gets if the treemap view should be hidden.</summary>
		public bool HideTreemap {
			get => (!Settings.ShowTreemap || (!Scanning.IsRefreshing && !Scanning.IsOpen)) && !IsInDesignMode;
		}
		/// <summary>Gets if the toolbar should be hidden.</summary>
		public bool HideToolBar => !Settings.ShowToolBar && !IsInDesignMode;
		/// <summary>Gets if the status bar should be hidden.</summary>
		public bool HideStatusBar => !Settings.ShowStatusBar && !IsInDesignMode;
		/// <summary>Gets if the graph view is enabled and able to refres.</summary>
		public bool GraphViewEnabled => Settings.ShowTreemap && !Scanning.IsRefreshing && Scanning.IsOpen;
		/// <summary>
		/// Gets if the elevate command should be hidden due to the process already being elevated.
		/// </summary>
		public bool HideElevateCommand => OS.IsElevated;

		#endregion

		#region Event Handlers

		private void OnRAMUsageTick() {
			GCRAMUsage = GC.GetTotalMemory(false);
		}

		private void OnStatusTick() {
			RaisePropertyChanged(nameof(ScanTime));
			RaisePropertyChanged(nameof(ScanProgress));
		}

		private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
			UI.Invoke(() => {
				switch (e.PropertyName) {
				case nameof(SettingsService.RAMInterval):
					ramTimer.Interval = Settings.RAMInterval;
					break;
				case nameof(SettingsService.StatusInterval):
					statusTimer.Interval = Settings.StatusInterval;
					break;
				case nameof(SettingsService.ShowFileTypes):
					RaisePropertyChanged(nameof(HideFileTypes));
					break;
				case nameof(SettingsService.ShowTreemap):
					RaisePropertyChanged(nameof(HideTreemap));
					RaisePropertyChanged(nameof(GraphViewEnabled));
					break;
				case nameof(SettingsService.ShowToolBar):
					RaisePropertyChanged(nameof(HideToolBar));
					break;
				case nameof(SettingsService.ShowStatusBar):
					RaisePropertyChanged(nameof(HideStatusBar));
					break;
				}
			});
		}

		private void OnScanningPropertyChanged(object sender, PropertyChangedEventArgs e) {
			UI.Invoke(() => {
				switch (e.PropertyName) {
				case nameof(ScanningService.RootItem):
					if (Scanning.RootItem == null) {
						GraphViewRootItem = null;
						RootItem = null;
					}
					else {
						RootItem = new FileItemViewModel(this, Scanning.RootItem);
					}
					break;
				case nameof(ScanningService.ProgressState):
					switch (Scanning.ProgressState) {
					case ScanProgressState.Starting:
						//GraphViewRootItem = null;
						break;
					case ScanProgressState.Started:
						statusTimer.Start();
						break;
					case ScanProgressState.Ending:
						statusTimer.Stop();
						break;
					case ScanProgressState.Ended:
						Extensions.Sort(ExtensionComparer.Compare);
						GraphViewRootItem = Scanning.RootItem;
						break;
					case ScanProgressState.NotStarted:
						GraphViewRootItem = null;
						RootItem?.Dispose();
						RootItem = null;
						//UpdateEmptyRecycleBin();
						break;
					}
					RaisePropertyChanged(nameof(Title));
					RaisePropertyChanged(nameof(ScanProgress));
					break;
				case nameof(ScanningService.ScanState):
					RaisePropertyChanged(nameof(HideFileTypes));
					RaisePropertyChanged(nameof(HideTreemap));
					RaisePropertyChanged(nameof(HideToolBar));
					RaisePropertyChanged(nameof(HideStatusBar));
					break;
				case nameof(ScanningService.ScanTime):
					RaisePropertyChanged(nameof(ScanTime));
					break;
				case nameof(ScanningService.Progress):
					RaisePropertyChanged(nameof(ScanProgress));
					break;
				case nameof(ScanningService.CanDisplayProgress):
					RaisePropertyChanged(nameof(CanDisplayScanProgress));
					break;
				case nameof(ScanningService.IsOpen):
					RaisePropertyChanged(nameof(IsOpen));
					RaisePropertyChanged(nameof(GraphViewEnabled));
					break;
				case nameof(ScanningService.IsScanning):
					RaisePropertyChanged(nameof(IsScanning));
					break;
				case nameof(ScanningService.IsRefreshing):
					RaisePropertyChanged(nameof(IsRefreshing));
					RaisePropertyChanged(nameof(GraphViewEnabled));
					//if (!Scanning.IsRefreshing)
					//	UpdateEmptyRecycleBin();
					break;
				case nameof(ScanningService.SuppressFileTreeRefresh):
					if (!suppressRefresh) {
						RootItem?.RaiseChildrenReset();
						RaisePropertyChanged(nameof(SuppressFileTreeRefresh));
					}
					break;
				}
			});
		}

		private void OnSelectedFilesChanged(object sender, NotifyCollectionChangedEventArgs e) {
			FileItemViewModel newSelectedFile = SelectedFiles.FirstOrDefault();
			if (SelectedFile != newSelectedFile) {
				SelectedFile = newSelectedFile;
				RaisePropertyChanged(nameof(SelectedFile));
				if ((SelectedFile == null) != (newSelectedFile == null))
					RaisePropertyChanged(nameof(HasFileSelection));
			}
		}

		#endregion

		#region IDisposable Implementation

		public void Dispose() {
			Scanning.Dispose();
			statusTimer.Stop();
			ramTimer.Stop();
		}

		#endregion

	}
}
