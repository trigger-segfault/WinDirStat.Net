using System;
using System.Diagnostics;
using System.Threading;
using WinDirStat.Net.Model.Data.Extensions;

namespace WinDirStat.Net.Model.Data {
	partial class WinDirStatModel {
		/// <summary>Callbacks for when a scan has ended in any fashion.</summary>
		public event ScanEventHander ScanEnded;
		/// <summary>Callbacks for <see cref="CancelAsync"/>.</summary>
		private event ScanEventHander ScanCancelled;

		private CancellationTokenSource cancel;
		private Thread scanThread;
		private Stopwatch watch;
		private readonly object threadLock = new object();
		private ThreadPriority scanPriority;

		public void Scan(params string[] rootPaths) {
			if (rootPaths == null)
				throw new ArgumentNullException(nameof(rootPaths));
			ThrowIfScanning();
			ScanPrepare();
			ScanThread(rootPaths, cancel.Token);
		}

		public void ScanAsync(ThreadPriority priority, params string[] rootPaths) {
			if (rootPaths == null)
				throw new ArgumentNullException(nameof(rootPaths));
			ThrowIfScanning();
			lock (threadLock) {
				ScanPriority = priority;
				ScanPrepare();
				scanThread = new Thread(() => ScanThread(rootPaths, cancel.Token)) {
					Priority = priority,
				};
				scanThread.Start();
			}
		}

		private void ScanThread(string[] rootPaths, CancellationToken token) {
			Exception exception = null;
			try {
				Scan(rootPaths, token);
			}
			catch (Exception ex) {
				exception = ex;
			}
			finally {
				ProgressState = ScanProgressState.Ending;
				cancel?.Dispose();
				cancel = null;
				watch?.Stop();
				scanThread = null;
				
				if (!disposed) {
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
					ProgressState = ScanProgressState.Ended;
					ScanEnded?.Invoke(this, new ScanEventArgs(scanState, progressState, exception));
					if (scanState == ScanState.Cancelled) {
						ScanCancelled?.Invoke(this, new ScanEventArgs(scanState, progressState, exception));
						ScanCancelled = null;
					}
				}
			}
		}

		private void ScanPrepare() {
			lock (threadLock) {
				ProgressState = ScanProgressState.Starting;
				ScanState = ScanState.Scanning;
				extensions.Clear();
				RootNode = null;
				scanningRootNode = null;
				scanningStates = null;
				GC.Collect();
				totalScannedSize = 0;
				watch = null;
				RaisePropertyChanged(nameof(ScanTime));
				// Create a new extension record collection so bindings aren't triggered
				// when the scan thread updates an extension.
				extensions.Clear();
				//RaisePropertyChanged(nameof(Extensions));
				watch = Stopwatch.StartNew();
				cancel = new CancellationTokenSource();
			}
		}

		[DebuggerStepThrough]
		private void ThrowIfScanning() {
			if (IsScanning)
				throw new InvalidOperationException("Cannot start new scan while one is currently in progress!");
		}


		/// <summary>
		/// Cancels the current scan if one exists.
		/// </summary>
		/// 
		/// <param name="waitForCancel">
		/// Waits for the scan to fully complete. DO NOT call this on the UI thread.
		/// </param>
		public void Cancel(bool waitForCancel = false) {
			if (IsScanning && ScanState != ScanState.Cancelling) {
				ScanState = ScanState.Cancelling;
				CancellationTokenSource cancel = this.cancel;
				ProgressState = ScanProgressState.Ending;
				cancel.Cancel();
				// Make sure the wait handle resumes if suspended.
				// Otherwise cancellation will never be reached.
				IsScanSuspended = false;
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
		/// <param name="runWhenNotScanning">True if the callback is run when no scan is running.</param>
		public void CancelAsync(Action callback, bool runWhenNotScanning = true) {
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			CancelAsync((o, e) => callback(), runWhenNotScanning);
		}

		/// <summary>
		/// Cancels the current scan if one exists and runs the callback if needed.
		/// </summary>
		/// 
		/// <param name="callback">The callback to run after cancellation is fininshed.</param>
		/// <param name="runWhenNotScanning">True if the callback is run when no scan is running.</param>
		public void CancelAsync(ScanEventHander callback, bool runWhenNotScanning = true) {
			if (callback == null)
				throw new ArgumentNullException(nameof(callback));
			if (IsScanning) {
				if (ScanState != ScanState.Cancelling) {
					ScanCancelled += new ScanEventHander(callback);
					Cancel(false);
				}
			}
			else if (runWhenNotScanning) {
				callback(this, new ScanEventArgs(scanState, progressState));
			}
		}

		private ScanEventArgs ScanArgs(Exception exception = null) {
			return new ScanEventArgs(scanState, progressState, exception);
		}

		public TimeSpan ScanTime {
			get => watch?.Elapsed ?? TimeSpan.Zero;
		}

		private volatile ScanState scanState;
		public ScanState ScanState {
			get => scanState;
			private set {
				if (scanState != value) {
					bool suspendedBefore = IsSuspended;
					bool scanningBefore = IsScanning;
					bool scanningNotRefreshingBefore = IsScanningAndNotRefreshing;
					bool finishedBefore = IsFinished;
					scanState = value;
					AutoRaisePropertyChanged();
					if (suspendedBefore != IsSuspended) {
						if (IsSuspended)
							watch.Stop();
						else
							watch.Start();
						IsScanSuspended = IsSuspended;
						RaisePropertyChanged(nameof(IsSuspended));
					}
					if (scanningBefore != IsScanning)
						RaisePropertyChanged(nameof(IsScanning));
					if (scanningNotRefreshingBefore != IsScanningAndNotRefreshing)
						RaisePropertyChanged(nameof(IsScanningAndNotRefreshing));
					if (finishedBefore != IsFinished)
						RaisePropertyChanged(nameof(IsFinished));
				}
			}
		}

		private volatile ScanProgressState progressState;
		public ScanProgressState ProgressState {
			get => progressState;
			private set {
				if (progressState != value) {
					progressState = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		private volatile bool canDisplayProgress;
		public bool CanDisplayProgress {
			get => canDisplayProgress;
			private set {
				if (canDisplayProgress != value) {
					canDisplayProgress = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public double Progress {
			get {
				if (canDisplayProgress) {
					switch (progressState) {
					case ScanProgressState.Started:
					case ScanProgressState.Ending:
						return (double) totalScannedSize / (scanTotalSize - scanTotalFreeSpace);
					case ScanProgressState.Ended:
						return 1d;
					}
				}
				return 0d;
			}
		}

		public bool IsScanning {
			get => scanState == ScanState.Scanning || scanState == ScanState.Suspended ||
				   scanState == ScanState.Cancelling;
		}

		public bool IsScanningAndNotRefreshing {
			get => (scanState == ScanState.Scanning || scanState == ScanState.Suspended ||
					scanState == ScanState.Cancelling) && !refreshing;
		}

		private volatile bool refreshing;
		public bool IsRefreshing {
			get => refreshing;
			set {
				if (refreshing != value) {
					refreshing = value;
					AutoRaisePropertyChanged();
				}
			}
		}

		public bool IsFinished {
			get => scanState == ScanState.Finished;
		}

		public bool IsSuspended {
			get => scanState == ScanState.Suspended;
			set {
				if (IsScanning && IsSuspended != value) {
					if (value)
						ScanState = ScanState.Suspended;
					else
						ScanState = ScanState.Scanning;
				}
			}
		}

		public bool IsAsync {
			get => scanThread != null && scanThread.IsAlive;
		}

		public ThreadPriority ScanPriority {
			get => scanPriority;
			set {
				if (scanPriority != value) {
					scanPriority = value;
					if (scanThread?.IsAlive ?? false)
						scanThread.Priority = value;
					AutoRaisePropertyChanged();
				}
			}
		}
	}
}
