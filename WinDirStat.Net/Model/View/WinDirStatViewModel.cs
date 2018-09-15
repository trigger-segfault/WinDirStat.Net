using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WinDirStat.Net.Model.Data;
using WinDirStat.Net.Model.Data.Nodes;
using WinDirStat.Net.Model.Settings;
using WinDirStat.Net.Model.View.Comparers;
using WinDirStat.Net.Model.View.Extensions;
using WinDirStat.Net.Model.View.Nodes;

namespace WinDirStat.Net.Model.View {
	public partial class WinDirStatViewModel : ViewModel, IDisposable {

		private readonly WinDirStatModel model;
		private readonly WinDirStatSettings settings;
		private readonly ExtensionRecordsViewModel extensions;
		private readonly IconCache icons;
		private readonly ObservableCollection<FileNodeViewModel> selectedFiles;
		private readonly ExtensionComparer extensionComparer;
		private readonly FileComparer fileComparer;
		private FileNodeViewModel rootNode;
		private FileNodeViewModel selectedFile;
		private ExtensionRecordViewModel selectedExtension;
		private readonly DispatcherTimer statusTimer;
		private readonly DispatcherTimer ramTimer;
		private readonly DispatcherTimer validateTimer;
		private long ramUsage;
		private double progress;
		private RootNode graphRootNode;

		public WinDirStatViewModel() {
			model = new WinDirStatModel();
			settings = new WinDirStatSettings(this);
			extensions = new ExtensionRecordsViewModel(this);
			icons = new IconCache(this);
			fileComparer = new FileComparer();
			extensionComparer = new ExtensionComparer();
			selectedFiles = new ObservableCollection<FileNodeViewModel>();
			selectedExtension = null;
			model.PropertyChanged += OnModelPropertyChanged;
			selectedFiles.CollectionChanged += OnSelectedFilesChanged;
			settings.PropertyChanged += OnSettingsPropertyChanged;
			ramTimer = new DispatcherTimer(
				settings.RAMInterval,
				DispatcherPriority.Normal,
				OnRAMUsageTick,
				Application.Current.Dispatcher);
			statusTimer = new DispatcherTimer(
				settings.StatusInterval,
				DispatcherPriority.Normal,
				OnStatusTick,
				Application.Current.Dispatcher);
			statusTimer.Stop();
			validateTimer = new DispatcherTimer(
				settings.ValidateInterval,
				DispatcherPriority.Background,
				OnValidateTick,
				Application.Current.Dispatcher);
			validateTimer.Stop();

			ramUsage = GC.GetTotalMemory(false);
		}

		public void Dispose() {
			model.Cancel(false);
			model.Dispose();
			validateTimer.Stop();
			statusTimer.Stop();
			ramTimer.Stop();
		}

		private void OnStatusTick(object sender, EventArgs e) {
			if (IsScanning) {
				Progress = model.Progress;
				RaisePropertyChanged(nameof(ScanTime));
			}
		}

		private void OnRAMUsageTick(object sender, EventArgs e) {
			RAMUsage = GC.GetTotalMemory(false);
		}

		private void OnValidateTick(object sender, EventArgs e) {
			if (IsScanning && rootNode != null) {
				// Reset the timer to make sure at least <interval> seconds pass by after each update is finished
				validateTimer.Stop();
				((RootNode) rootNode.Model).Validate(false);// (rootNode, false);
				//extensions.Validate();
				validateTimer.Start();
			}
		}

		public WinDirStatModel Model {
			get => model;
		}

		public FileNodeViewModel RootNode {
			get => rootNode;
			private set {
				if (rootNode != value) {
					rootNode = value;
					if (rootNode != null) {
						rootNode.IsExpanded = true;
					}
					AutoRaisePropertyChanged();
				}
			}
		}
		public RootNode GraphRootNode {
			get => graphRootNode;
			private set {
				if (graphRootNode != value) {
					graphRootNode = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public long RAMUsage {
			get => ramUsage;
			private set {
				if (ramUsage != value) {
					ramUsage = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public ObservableCollection<FileNodeViewModel> SelectedFiles {
			get => selectedFiles;
		}

		public FileNodeViewModel SelectedFile {
			get => selectedFile;
			set {
				if (selectedFiles.Count != 1 || selectedFiles.FirstOrDefault() != selectedFile) {
					// Property change will be raised by OnSelectedFilesChanged
					if (selectedFiles.Count == 0) {
						selectedFiles.Add(value);
					}
					else {
						selectedFiles[0] = value;
						while (selectedFiles.Count > 1) {
							selectedFiles.RemoveAt(1);
						}
					}
				}
			}
		}

		public bool HasFileSelection {
			get => selectedFile != null;
		}

		public ExtensionRecordViewModel SelectedExtension {
			get => selectedExtension;
			set {
				if (selectedExtension != value) {
					bool hasSelectionChanged = ((selectedExtension == null) != (value == null));
					selectedExtension = value;
					AutoRaisePropertyChanged();
					if (hasSelectionChanged)
						RaisePropertyChanged(nameof(HasExtensionSelection));
				}
			}
		}

		public bool HasExtensionSelection {
			get => selectedExtension != null;
		}

		public ExtensionRecordsViewModel Extensions {
			get => extensions;
		}

		public IconCache Icons {
			get => icons;
		}

		public WinDirStatSettings Settings {
			get => settings;
		}

		public FileComparer FileComparer {
			get => fileComparer;
		}

		public ExtensionComparer ExtensionComparer {
			get => extensionComparer;
		}

		private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
			Application.Current.Dispatcher.Invoke(() => {
				switch (e.PropertyName) {
				case nameof(WinDirStatModel.RootNode):
					if (model.RootNode == null) {
						RootNode = null;
						GraphRootNode = null;
					}
					else {
						RootNode = new FileNodeViewModel(this, model.RootNode);
					}
					break;
				case nameof(WinDirStatModel.ProgressState):
					switch (model.ProgressState) {
					case ScanProgressState.Starting:
						extensions.IsScanning = true;
						GraphRootNode = null;
						break;
					case ScanProgressState.Started:
						statusTimer.Start();
						validateTimer.Start();
						GC.Collect();
						break;
					case ScanProgressState.Ending:
						statusTimer.Stop();
						validateTimer.Stop();
						break;
					case ScanProgressState.Ended:
						extensions.IsScanning = false;
						extensions.PrepareForDisplay(extensionComparer.Compare);
						GraphRootNode = model.RootNode;
						break;
					}
					CommandManager.InvalidateRequerySuggested();
					break;
				case nameof(WinDirStatModel.IsFinished):
				case nameof(WinDirStatModel.IsScanning):
				case nameof(WinDirStatModel.IsRefreshing):
					CommandManager.InvalidateRequerySuggested();
					RaisePropertyChanged(nameof(HideFileTypes));
					RaisePropertyChanged(nameof(HideTreemap));
					RaisePropertyChanged(nameof(GraphViewEnabled));
					break;
				}
				RaisePropertyChanged(e.PropertyName);
			});
		}

		private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			case nameof(WinDirStatSettings.ShowFreeSpace):
				IsRefreshing = true;
				model.ShowFreeSpace = settings.ShowFreeSpace;
				IsRefreshing = false;
				break;
			case nameof(WinDirStatSettings.ShowUnknown):
				IsRefreshing = true;
				model.ShowUnknown = settings.ShowUnknown;
				IsRefreshing = false;
				break;
			case nameof(WinDirStatSettings.RAMInterval):
				ramTimer.Interval = settings.RAMInterval;
				break;
			case nameof(WinDirStatSettings.StatusInterval):
				statusTimer.Interval = settings.StatusInterval;
				break;
			case nameof(WinDirStatSettings.ValidateInterval):
				validateTimer.Interval = settings.ValidateInterval;
				break;
			case nameof(WinDirStatSettings.ScanPriority):
				model.ScanPriority = settings.ScanPriority;
				break;
			case nameof(WinDirStatSettings.ShowFileTypes):
				RaisePropertyChanged(nameof(HideFileTypes));
				break;
			case nameof(WinDirStatSettings.ShowTreemap):
				RaisePropertyChanged(nameof(HideTreemap));
				RaisePropertyChanged(nameof(GraphViewEnabled));
				break;
			}
		}

		private void OnSelectedFilesChanged(object sender, NotifyCollectionChangedEventArgs e) {
			FileNodeViewModel newSelectedFile = selectedFiles.FirstOrDefault();
			if (selectedFile != newSelectedFile) {
				selectedFile = newSelectedFile;
				RaisePropertyChanged(nameof(SelectedFile));
				if ((selectedFile == null) != (newSelectedFile == null))
					RaisePropertyChanged(nameof(HasFileSelection));
			}
			CommandManager.InvalidateRequerySuggested();
		}
		
		public bool HideFileTypes {
			get => !settings.ShowFileTypes || (!IsRefreshing && !IsFinished);
		}
		public bool HideTreemap {
			get => !settings.ShowTreemap || (!IsRefreshing && !IsFinished);
		}

		public bool GraphViewEnabled {
			get => settings.ShowTreemap && !IsRefreshing && !IsScanning;
		}
	}
}
