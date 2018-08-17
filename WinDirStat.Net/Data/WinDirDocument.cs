using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WinDirStat.Net.Data.Nodes;
using WinDirStat.Net.Drawing;
using WinDirStat.Net.Settings;
using WinDirStat.Net.Windows;

namespace WinDirStat.Net.Data {
	public enum ScanState {
		NotStarted,
		Scanning,
		Refreshing,
		Suspended,
		Finished,
		Cancelling,
		Cancelled,
		Failed,
	}

	public delegate void ScanEventHander(object sender, ScanEventArgs e);
	public delegate void RootNodeEventHander(object sender, RootNode rootNode);
	public delegate void ExceptionEventHandler(object sender, Exception ex);
	public class ScanEventArgs {
		public ScanState ScanState { get; }
		public Exception Exception { get; }

		public ScanEventArgs(ScanState scanState) {
			ScanState = scanState;
			Exception = null;
		}

		public ScanEventArgs(ScanState scanState, Exception exception) {
			ScanState = scanState;
			Exception = exception;
		}
	}
	public class ExceptionEventArgs {
		public Exception Exception { get; }

		public ExceptionEventArgs(Exception exception) {
			Exception = exception;
		}
	}

	public class WinDirDocument : IDisposable, INotifyPropertyChanged {
		
		//private static readonly TimeSpan DefaultProgressInterval = TimeSpan.FromSeconds(0.05);
		//private static readonly TimeSpan DefaultValidateInterval = TimeSpan.FromSeconds(2.5);
		//private static readonly TimeSpan DefaultRAMUsageInterval = TimeSpan.FromSeconds(1.0);

		private ExtensionRecords extensions;
		private readonly IconCache icons;
		private readonly FileEnumerator fileEnumerator;
		private FileNodeComparer fileComparer;
		private ExtensionRecordComparer extensionComparer;
		private CancellationTokenSource cancel;
		private Thread scanThread;
		private readonly object threadLock = new object();
		private Stopwatch watch;
		private volatile ScanState scanState;
		private RootNode rootNode;
		private double progress;
		private bool hasProgress;
		private readonly DispatcherTimer statusTimer;
		private readonly DispatcherTimer ramTimer;
		private readonly DispatcherTimer validateTimer;
		private long ramUsage;
		private Action cancelCallback;
		private WinDirSettings settings;
		private bool disposed;
		private readonly SelectDrivesDocument selectDrives;

		public event PropertyChangedEventHandler PropertyChanged;
		public event ScanEventHander ScanRootSetup;
		public event ScanEventHander ScanEnded;

		public WinDirDocument() {
			selectDrives = new SelectDrivesDocument(this);
			settings = new WinDirSettings(this);
			settings.PropertyChanged += SettingsPropertyChanged;
			extensions = new ExtensionRecords(this);
			icons = new IconCache(this);
			fileEnumerator = new FileEnumerator(this);
			fileComparer = new FileNodeComparer();
			extensionComparer = new ExtensionRecordComparer();
			scanState = ScanState.NotStarted;
			rootNode = null;
			statusTimer = new DispatcherTimer(
				settings.StatusInterval,
				DispatcherPriority.Normal,
				OnStatusTick,
				Application.Current.Dispatcher);
			ramTimer = new DispatcherTimer(
				settings.RAMInterval,
				DispatcherPriority.Normal,
				OnRAMUsageTick,
				Application.Current.Dispatcher);
			validateTimer = new DispatcherTimer(
				settings.ValidateInterval,
				DispatcherPriority.Background,
				OnValidateTick,
				Application.Current.Dispatcher);
			validateTimer.Stop();

			ramUsage = GC.GetTotalMemory(false);
		}

		private void OnStatusTick(object sender, EventArgs e) {
			if (IsScanningOrRefreshing) {
				HasProgress = fileEnumerator.HasProgress;
				Progress = fileEnumerator.Progress;
				rootNode?.RaisePropertyChanged(nameof(FileNodeBase.Percent));
				RaisePropertyChanged(nameof(ScanTime));
			}
		}

		private void OnRAMUsageTick(object sender, EventArgs e) {
			RAMUsage = GC.GetTotalMemory(false);
		}

		private void OnValidateTick(object sender, EventArgs e) {
			if (IsScanningOrRefreshing && rootNode != null) {
				// Reset the timer to make sure at least 2.5 seconds pass by after each update is finished
				validateTimer.Stop();
				rootNode.Validate(rootNode, false);
				//extensions.Validate();
				validateTimer.Start();
			}
		}

		public void Scan(params string[] rootPaths) {
			if (rootPaths == null)
				throw new ArgumentNullException(nameof(rootPaths));
			ThrowIfScanningOrRefreshing();
			ScanPrepare();
			ScanThread(rootPaths, cancel.Token);
		}

		public void ScanAsync(params string[] rootPaths) {
			if (rootPaths == null)
				throw new ArgumentNullException(nameof(rootPaths));
			ThrowIfScanningOrRefreshing();
			lock (threadLock) {
				ScanPrepare();
				scanThread = new Thread(() => ScanThread(rootPaths, cancel.Token)) {
					Priority = settings.ScanPriority,
				};
				scanThread.Start();
			}
		}

		private void ScanThread(string[] rootPaths, CancellationToken token) {
			Exception exception = null;
			try {
				validateTimer.Start();
				fileEnumerator.Scan(rootPaths, OnRootSetup, token);
			}
			catch (Exception ex) {
				exception = ex;
			}
			finally {
				validateTimer?.Stop();
				cancel?.Dispose();
				cancel = null;
				watch?.Stop();
				scanThread = null;

				if (ScanState != ScanState.Cancelling)
					Progress = 1d;
				Application.Current.Dispatcher.Invoke(() => {
					if (disposed)
						return;
					RaisePropertyChanged(nameof(ScanTime));
					if (scanState == ScanState.Cancelling) {
						RootNode = null;
						ScanState = ScanState.Cancelled;
					}
					else if (exception == null) {
						ScanState = ScanState.Finished;
					}
					else {
						ScanState = ScanState.Failed;
					}
					ScanEnded?.Invoke(this, new ScanEventArgs(scanState, exception));

					cancelCallback?.Invoke();
					cancelCallback = null;
				});
			}
		}

		private void ScanPrepare() {
			lock (threadLock) {
				watch = null;
				RaisePropertyChanged(nameof(ScanTime));
				RootNode = null;
				Progress = 0d;
				// Create a new extension record collection so bindings aren't triggered
				// when the scan thread updates an extension.
				extensions?.Clear();
				extensions = null;
				extensions = new ExtensionRecords(this);
				//RaisePropertyChanged(nameof(Extensions));
				ScanState = ScanState.Scanning;
				watch = Stopwatch.StartNew();
				cancel = new CancellationTokenSource();
			}
		}
	
		private void OnRootSetup(RootNode rootNode) {
			if (scanState != ScanState.Cancelling) {
				RootNode = rootNode;
				ScanRootSetup?.Invoke(this, new ScanEventArgs(scanState));
			}
		}

		private void ThrowIfScanningOrRefreshing() {
			if (IsScanningOrRefreshing)
				throw new InvalidOperationException("Cannot start new scan while one is currently in progress!");
		}

		public void Dispose() {
			disposed = true;
			Cancel(false);
			statusTimer.Stop();
			ramTimer.Stop();
		}

		/// <summary>
		/// Cancels the current scan if one exists.
		/// </summary>
		/// 
		/// <param name="waitForCancel">
		/// Waits for the scan to fully complete. DO NOT call this on the UI thread.
		/// </param>
		public void Cancel(bool waitForCancel = false) {
			if (IsScanningOrRefreshing && ScanState != ScanState.Cancelling) {
				ScanState = ScanState.Cancelling;
				CancellationTokenSource cancel = this.cancel;
				validateTimer.Stop();
				cancel.Cancel();
				// Make sure the wait handle resumes if suspended.
				// Otherwise cancellation will never be reached.
				fileEnumerator.IsSuspended = false;
				while (waitForCancel && IsAsync)
					Thread.Sleep(5);
			}
			//icons.Cancel();
		}

		/// <summary>
		/// Cancels the current scan if one exists and runs the callback if needed.
		/// </summary>
		/// 
		/// <param name="callback">The callback to run after cancellation is fininshed.</param>
		/// <param name="runWhenCancelled">True if the callback is run when no scan is running.</param>
		public void CancelAsync(Action callback, bool runWhenCancelled = true) {
			if (IsScanningOrRefreshing) {
				if (ScanState != ScanState.Cancelling) {
					cancelCallback = callback;
					Cancel(false);
				}
			}
			else {
				callback();
			}
		}

		internal void FinalValidate() {
			Application.Current.Dispatcher.Invoke(extensions.FinalValidation);
			//extensions.FinalValidation();
			RootNode.FinalValidate(RootNode);
		}

		public TimeSpan ScanTime {
			get => watch?.Elapsed ?? TimeSpan.Zero;
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

		public ScanState ScanState {
			get => scanState;
			private set {
				if (scanState != value) {
					bool suspendedBefore = IsSuspended;
					bool scanningBefore = IsScanning;
					bool scanningOrRefreshingBefore = IsScanningOrRefreshing;
					bool finishedBefore = IsFinished;
					scanState = value;
					AutoRaisePropertyChanged();
					if (scanningBefore != IsScanning)
						RaisePropertyChanged(nameof(IsScanning));
					if (suspendedBefore != IsSuspended)
						RaisePropertyChanged(nameof(IsSuspended));
					if (scanningOrRefreshingBefore != IsScanningOrRefreshing)
						RaisePropertyChanged(nameof(IsScanningOrRefreshing));
					if (finishedBefore != IsFinished) {
						RaisePropertyChanged(nameof(IsFinished));
						RaisePropertyChanged(nameof(FinishedRootNode));
						RaisePropertyChanged(nameof(FinishedExtensions));
					}
				}
			}
		}

		public bool HasProgress {
			get => hasProgress;
			private set {
				if (hasProgress != value) {
					hasProgress = value;
					AutoRaisePropertyChanged();
				}
			}
		}
		
		public double Progress {
			get => progress;
			private set {
				if (progress != value) {
					progress = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public bool IsScanningOrRefreshing {
			get => scanState == ScanState.Scanning || scanState == ScanState.Suspended ||
				scanState == ScanState.Cancelling || scanState == ScanState.Refreshing;
		}

		public bool IsScanning {
			get => scanState == ScanState.Scanning || scanState == ScanState.Suspended ||
				scanState == ScanState.Cancelling;
		}

		public bool IsRefreshing {
			get => scanState == ScanState.Refreshing;
		}

		public bool IsFinished {
			get => scanState == ScanState.Finished;
		}

		public bool IsSuspended {
			get => scanState == ScanState.Suspended;
			set {
				if (IsScanningOrRefreshing && IsSuspended != value) {
					if (value) {
						ScanState = ScanState.Suspended;
						watch.Stop();
					}
					else {
						ScanState = ScanState.Scanning;
						watch.Start();
					}
					fileEnumerator.IsSuspended = value;
					//RaisePropertyChanged(nameof(ScanState));
					//AutoRaisePropertyChanged();
				}
			}
		}

		public bool IsAsync {
			get => scanThread != null && scanThread.IsAlive;
		}

		public FileSortMethod FileSortMethod {
			get => fileComparer.SortMethod;
			set {
				if (FileSortMethod != value) {
					fileComparer.SortMethod = value;
					RootNode.Sort(RootNode, true);
					AutoRaisePropertyChanged();
				}
			}
		}

		public ListSortDirection FileSortDirection {
			get => fileComparer.SortDirection;
			set {
				if (FileSortDirection != value) {
					fileComparer.SortDirection = value;
					RootNode.Sort(RootNode, true);
					AutoRaisePropertyChanged();
				}
			}
		}

		public void SetFileSort(FileSortMethod method, ListSortDirection direction) {
			bool methodChanged = FileSortMethod != method;
			bool directionChanged = FileSortDirection != direction;
			fileComparer.SortMethod = method;
			fileComparer.SortDirection = direction;
			if (methodChanged || directionChanged)
				rootNode?.Sort(rootNode, true);
			if (methodChanged)
				RaisePropertyChanged(nameof(FileSortMethod));
			if (directionChanged)
				RaisePropertyChanged(nameof(FileSortDirection));
		}

		public WinDirSettings Settings {
			get => settings;
		}

		internal FileNodeComparer FileComparer {
			get => fileComparer;
		}

		public ExtensionSortMethod ExtensionSortMethod {
			get => extensionComparer.SortMethod;
			set {
				if (ExtensionSortMethod != value) {
					extensionComparer.SortMethod = value;
					extensions.Sort();
					AutoRaisePropertyChanged();
				}
			}
		}

		public ListSortDirection ExtensionSortDirection {
			get => extensionComparer.SortDirection;
			set {
				if (ExtensionSortDirection != value) {
					extensionComparer.SortDirection = value;
					extensions.Sort();
					AutoRaisePropertyChanged();
				}
			}
		}

		public void SetExtensionSort(ExtensionSortMethod method, ListSortDirection direction) {
			bool methodChanged = ExtensionSortMethod != method;
			bool directionChanged = ExtensionSortDirection != direction;
			extensionComparer.SortMethod = method;
			extensionComparer.SortDirection = direction;
			if (methodChanged || directionChanged)
				extensions.Sort();
			if (methodChanged)
				RaisePropertyChanged(nameof(ExtensionSortMethod));
			if (directionChanged)
				RaisePropertyChanged(nameof(ExtensionSortDirection));
		}

		public SelectDrivesDocument SelectDrives {
			get => selectDrives;
		}

		internal ExtensionRecordComparer ExtensionComparer {
			get => extensionComparer;
		}

		internal FileNodeComparer SizeFileComparer {
			get => FileNodeComparer.Default;
		}

		public ExtensionRecords Extensions {
			get => extensions;
		}

		public IconCache Icons {
			get => icons;
		}

		public RootNode RootNode {
			get => rootNode;
			private set {
				if (rootNode != value) {
					rootNode = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		private void RaisePropertyChanged(string name) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void AutoRaisePropertyChanged([CallerMemberName] string name = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public ExtensionRecords FinishedExtensions {
			get => (ScanState == ScanState.Finished ? extensions : null);
		}

		public RootNode FinishedRootNode {
			get => (ScanState == ScanState.Finished ? rootNode : null);
		}

		public void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
			case nameof(WinDirSettings.ShowFreeSpace):
				if (rootNode != null)
					rootNode.OnShowFreeSpaceChanged(settings.ShowFreeSpace);
				break;
			case nameof(WinDirSettings.ShowUnknown):
				if (rootNode != null)
					rootNode.OnShowUnknownChanged(settings.ShowUnknown);
				break;
			case nameof(WinDirSettings.RAMInterval):
				ramTimer.Interval = settings.RAMInterval;
				break;
			case nameof(WinDirSettings.StatusInterval):
				statusTimer.Interval = settings.StatusInterval;
				break;
			case nameof(WinDirSettings.ValidateInterval):
				validateTimer.Interval = settings.ValidateInterval;
				break;
			case nameof(WinDirSettings.ScanPriority):
				if (scanThread?.IsAlive ?? false)
					scanThread.Priority = settings.ScanPriority;
				break;
			}
		}
	}
}
