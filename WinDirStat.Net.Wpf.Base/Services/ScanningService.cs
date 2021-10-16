using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Drives;
using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Model.Files;

namespace WinDirStat.Net.Services {
    /// <summary>A service for scanning a path's file tree.</summary>
    public partial class ScanningService : ObservableVolatileObject, IAsyncDisposable {

        #region Protected Classes

        /// <summary>A collection of parent folders and files to refresh.</summary>
        protected class RefreshFiles {
            /// <summary>The parent folder of the files.</summary>
            public FolderItem Parent { get; }
            /// <summary>The files in the parent folder to refresh.</summary>
            public List<FileItemBase> Files { get; }

            /// <summary>Constructs the <see cref="RefreshFiles"/>.</summary>
            public RefreshFiles(FolderItem parent) {
                Parent = parent;
                Files = new List<FileItemBase>();
            }

            /// <summary>Constructs the <see cref="RefreshFiles"/>.</summary>
            public RefreshFiles(KeyValuePair<FolderItem, List<FileItemBase>> pair) {
                Parent = pair.Key;
                Files = pair.Value;
            }
        }

        #endregion

        #region Fields

        /// <summary>The program settings.</summary>
        protected readonly SettingsService settings;
        /// <summary>The OS specific service</summary>
        protected readonly OSService os;
        /// <summary>The UI service</summary>
        protected readonly UIService ui;

        // Scan async
        /// <summary>The cancellation token source for the asynchronous scan thread.</summary>
        private CancellationTokenSource cancel;
        /// <summary>The current asynchronous scan thread.</summary>
        private Task scanTask;
        /// <summary>The lock object for scan thread setup.</summary>
        private readonly object threadLock = new object();
        /// <summary>The lock object exclusively for accessing the resume event.</summary>
        private readonly object resumeLock = new object();
        /// <summary>The timer for validating the file tree during a scan.</summary>
        private readonly Timer validateTimer;

        // Scan progress
        /// <summary>The total size that has been scanned so far.</summary>
        private long totalScannedSize;
        /// <summary>The total size for all scanned roots.</summary>
        private long totalSize;
        /// <summary>The total free space for all scanned roots.</summary>
        private long totalFreeSpace;
        /// <summary>True if all scanned roots are drives and thus have a definite used size.</summary>
        private bool canDisplayProgress;
        /// <summary>The watch for keeping track of the scan duration.</summary>
        private readonly Stopwatch scanWatch;
        /// <summary>A watch that keeps track of the time spent validating.</summary>
        private readonly Stopwatch validateWatch;
        /// <summary>True if validation has been requested on the scan thread.</summary>
        private volatile bool validationRequested;

        // Scan state
        /// <summary>The current state of the scan.</summary>
        private ScanState scanState;
        /// <summary>The current progress state of the scan.</summary>
        private ScanProgressState progressState;
        /// <summary>True if a refresh operation is in progress.</summary>
        private bool isRefreshing;
        /// <summary>The wait handle for resuming a scan. Set if the scan is not suspended.</summary>
        private ManualResetEvent resumeEvent = new ManualResetEvent(true);
        /// <summary>True if the UI refreshing should be supressed due to validation.</summary>
        private bool suppressRefresh;

        // Scan result
        /// <summary>The root item of the file tree being scanned.</summary>
        private RootItem rootItem;
        /// <summary>The resulting exception from the current scan.</summary>
        private Exception exceptionResult;
        /// <summary>Gets the extension item collection that records all encountered extensions.</summary>
        public ExtensionItems Extensions { get; }
        /// <summary>Gets the drive item collection that records the current drive list.</summary>
        public DriveItems Drives { get; }
        /// <summary>The root paths to scan.</summary>
        private string[] rootPaths;
        /// <summary>The files to refresh.</summary>
        private RefreshFiles[] refreshFiles;
        /// <summary>The result of the drive select dialog used for scanning.</summary>
        private DriveSelectResult driveSelectResult;

        // Dispose
        /// <summary>True if the scanning service has been disposed of.</summary>
        private volatile bool disposed;

        #endregion

        #region Events

        /// <summary>Callbacks for when a scan has ended in any fashion.</summary>
        public event ScanEventHander Ended;
        /// <summary>Callbacks for <see cref="CancelAsync"/>.</summary>
        private event ScanEventHander Cancelled;

        /// <summary>Called when the space settings have changed.</summary>
        public event EventHandler SpaceChanged;

        #endregion

        #region Constructors

        /// <summary>Constructs the <see cref="ScanningService"/>.</summary>
        public ScanningService(SettingsService settings,
                               OSService os,
                               UIService ui) {
            this.settings = settings;
            this.os = os;
            this.ui = ui;
            settings.PropertyChanged += OnSettingsPropertyChanged;
            Extensions = new ExtensionItems(this, settings);
            Drives = new DriveItems(this);
            validateTimer = new Timer(OnValidateTick, null, Timeout.Infinite, Timeout.Infinite);
            validateWatch = new Stopwatch();
            scanWatch = new Stopwatch();
            validationRequested = false;
        }

        #endregion

        #region Event Handlers

        /// <summary>Called when the settings' properties have changed.</summary>
        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SettingsService.ShowFreeSpace):
                case nameof(SettingsService.ShowUnknown):
                case nameof(SettingsService.ShowTotalSpace):
                    bool shouldSet = !IsScanning && !IsRefreshing;
                    if (shouldSet)
                        IsRefreshing = true;
                    OnPropertyChanged(e.PropertyName);
                    RaiseSpaceChanged();
                    if (shouldSet)
                        IsRefreshing = false;
                    break;
                case nameof(SettingsService.ScanPriority):
                    lock (volatileLock) {
                        if (IsAsync)
                            throw new NotImplementedException();
                    }
                    break;
                case nameof(SettingsService.ValidateInterval):
                    // Same thing as performing an interval change
                    StartValidateTimer();
                    break;
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>Stops the validate timer.</summary>
        private void StopValidateTimer() {
            if (!disposed)
                validateTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>Starts the validate timer.</summary>
        private void StartValidateTimer() {
            if (!disposed)
                validateTimer.Change(settings.ValidateInterval, Timeout.InfiniteTimeSpan);
        }

        /// <summary>Adds a request for the file tree to be validated.</summary>
        private void OnValidateTick(object state) {
            validationRequested = true;
        }

        /// <summary>Raises the <see cref="SpaceChanged"/> event.</summary>
        private void RaiseSpaceChanged() {
            SpaceChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Throws an exception if a scan is in progress.</summary>
        [DebuggerStepThrough]
        private void ThrowIfScanning() {
            if (IsScanning)
                throw new InvalidOperationException("Cannot start new scan while one is currently in progress!");
        }

        #endregion

        #region Public Scanning

        /// <summary>Begins a synchronous scan of the specified root paths.</summary>
        public void Scan(DriveSelectResult result) {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            ThrowIfScanning();
            lock (threadLock) {
                RootPaths = result.GetResultPaths();
                driveSelectResult = result;
                ScanPrepare(false);
                ScanThread(false, cancel.Token);
            }
        }

        /// <summary>Begins a asynchronous scan of the specified root paths.</summary>
        public void ScanAsync(DriveSelectResult result) {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            ThrowIfScanning();
            lock (threadLock) {
                RootPaths = result.GetResultPaths();
                driveSelectResult = result;
                ScanPrepare(false);
                Debug.Assert(scanTask == null || scanTask.IsCompleted);
                scanTask = Task.Run(() => ScanThread(false, cancel.Token)); //{
            }
        }

        /// <summary>Closes the current scanned file tree.</summary>
        /// 
        /// <param name="waitForClose">
        /// Waits for the scan to fully close. DO NOT call this on the UI thread.
        /// </param>
        public void Close(bool waitForClose = false) {
            if (IsOpen) {
                Cancel(waitForClose);
                RootItem = null;
                ClosedCleanup();
                ScanState = ScanState.NotStarted;
                ProgressState = ScanProgressState.NotStarted;
                Extensions.Clear();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        /// <summary>
        /// Closes the current scanned file tree if one exists and runs the callback if needed.
        /// </summary>
        /// 
        /// <param name="callback">The callback to run after close has fininshed.</param>
        /// <param name="runWhenNotOpen">True if the callback is run when no file tree is open.</param>
        public void CloseAsync(Action callback, bool runWhenNotOpen = true) {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (IsScanning) {
                CancelAsync(() => {
                    Close(true);
                    callback();
                }, true);
            }
            else if (IsOpen || runWhenNotOpen) {
                Close(true);
                callback();
            }
        }

        /// <summary>Cancels the current scan if one exists.</summary>
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
                IsSuspended = false;
                while (waitForCancel && IsAsync)
                    Thread.Sleep(5);
            }
        }

        /// <summary>Cancels the current scan if one exists and runs the callback if needed.</summary>
        /// 
        /// <param name="callback">The callback to run after cancellation is fininshed.</param>
        /// <param name="runWhenNotScanning">True if the callback is run when no scan is running.</param>
        public void CancelAsync(Action callback, bool runWhenNotScanning = true) {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            CancelAsync((o, e) => callback(), runWhenNotScanning);
        }

        /// <summary>Cancels the current scan if one exists and runs the callback if needed.</summary>
        /// 
        /// <param name="callback">The callback to run after cancellation is fininshed.</param>
        /// <param name="runWhenNotScanning">True if the callback is run when no scan is running.</param>
        public void CancelAsync(ScanEventHander callback, bool runWhenNotScanning = true) {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (IsScanning) {
                if (ScanState != ScanState.Cancelling) {
                    Cancelled += new ScanEventHander(callback);
                    Cancel(false);
                }
            }
            else if (runWhenNotScanning) {
                callback(this, new ScanEventArgs(scanState, progressState));
            }
        }

        #endregion

        #region Scan Prepare/Thread

        /// <summary>Prepares the scan before starting.</summary>
        private void ScanPrepare(bool refreshing) {
            lock (threadLock) {
                IsSuspended = false;
                IsRefreshing = refreshing;
                refreshFiles = null;
                CanDisplayProgress = false;
                validateWatch.Reset();
                scanWatch.Reset();
                ProgressState = ScanProgressState.Starting;
                ScanState = ScanState.Scanning;
                TotalScannedSize = 0;
                TotalSize = 0;
                TotalFreeSpace = 0;
                if (!refreshing) {
                    Extensions.Clear();
                    RootItem = null;
                }
                validateWatch.Reset();
                OnPropertyChanged(nameof(ScanTime));
                scanWatch.Start();
                cancel = new CancellationTokenSource();
                StartValidateTimer();
            }
        }

        /// <summary>The root method for running the scan.</summary>
        private void ScanThread(bool useRefreshFiles, CancellationToken token) {
            Exception exception = null;
            try {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                if (useRefreshFiles)
                    Refresh(refreshFiles, token);
                else
                    Scan(rootPaths, token);
                RootItem?.FullValidate();
            }
            catch (ThreadAbortException ex) {
                exception = ex;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                exception = ex;
            }
            finally {
                refreshFiles = null;
                FinishedCleanup();
                StopValidateTimer();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                ProgressState = ScanProgressState.Ending;
                scanWatch?.Stop();

                if (!disposed) {
                    OnPropertyChanged(nameof(ScanTime));
                    if (scanState == ScanState.Cancelling) {
                        // TODO: Should cancel remove all progress made?
                        //RootItem = null;
                        ScanState = ScanState.Cancelled;
                    }
                    else if (exception == null) {
                        ScanState = ScanState.Finished;
                    }
                    else {
                        RootItem = null;
                        ExceptionResult = exception;
                        ScanState = ScanState.Failed;
                    }
                    IsRefreshing = false;
                    ProgressState = ScanProgressState.Ended;
                    Ended?.Invoke(this, new ScanEventArgs(scanState, progressState, exception));
                    if (scanState == ScanState.Cancelled) {
                        Cancelled?.Invoke(this, new ScanEventArgs(scanState, progressState, exception));
                        Cancelled = null;
                    }
                }
                else {
                    IsRefreshing = false;
                    ExceptionResult = exception;
                    ScanState = ScanState.Failed;
                }
            }
        }

        #endregion

        #region Root Space Properties

        /// <summary>Gets if free space items should be listed.</summary>
        public bool ShowFreeSpace => settings.ShowFreeSpace;
        /// <summary>Gets if unknown items should be listed.</summary>
        public bool ShowUnknown => settings.ShowUnknown;
        /// <summary>
        /// Gets if free space and unknown items should be listed in the absolute or file root.
        /// </summary>
        public bool ShowTotalSpace => settings.ShowTotalSpace;

        #endregion

        #region Scan Properties

        /*/// <summary>Gets the result of the drive select dialog used for scanning.</summary>
		public DriveSelectResult DriveSelectResult {
			get => driveSelectResult;
			set => SetProperty(ref driveSelectResult, value);
		}*/

        /// <summary>Gets the root paths to scan.</summary>
        public string[] RootPaths {
            get => rootPaths;
            set => SetProperty(ref rootPaths, value);
        }

        /// <summary>Gets the root item of the scanned/scanning file tree.</summary>
        public RootItem RootItem {
            get => VolatileGet(ref rootItem);
            protected set {
                lock (volatileLock) {
                    if (rootItem == value)
                        return;
                    // Unhooks the events from the IScanningService
                    rootItem?.Dispose();
                    rootItem = value;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>Gets how long the scan has been going on for. Or how long the scan took.</summary>
        public TimeSpan ScanTime => scanWatch.Elapsed;

        /// <summary>Gets the amount of time taken to validate the file tree while scanning.</summary>
        public TimeSpan ValidateTime => validateWatch.Elapsed;

        /// <summary>Gets the current state of the scan.</summary>
        public ScanState ScanState {
            get => scanState;
            set {
                bool suspendedChanged = false;
                bool scanningChanged = false;
                bool scanningNotRefreshingChanged = false;
                bool finishedChanged = false;
                bool openChanged = false;
                lock (volatileLock) {
                    if (scanState == value)
                        return;

                    bool suspendedBefore = IsSuspended;
                    bool scanningBefore = IsScanning;
                    bool scanningNotRefreshingBefore = IsScanningAndNotRefreshing;
                    bool finishedBefore = IsFinished;
                    bool openBefore = IsOpen;

                    scanState = value;

                    suspendedChanged = suspendedBefore != IsSuspended;
                    scanningChanged = scanningBefore != IsScanning;
                    scanningNotRefreshingChanged = scanningNotRefreshingBefore != IsScanningAndNotRefreshing;
                    finishedChanged = finishedBefore != IsFinished;
                    openChanged = openBefore != IsOpen;

                    if (suspendedChanged) {
                        if (IsSuspended)
                            scanWatch.Stop();
                        else
                            scanWatch.Start();
                    }
                }
                OnPropertyChanged();
                OnPropertyChangedIf(suspendedChanged, nameof(IsSuspended));
                OnPropertyChangedIf(scanningChanged, nameof(IsScanning));
                OnPropertyChangedIf(scanningNotRefreshingChanged, nameof(IsScanningAndNotRefreshing));
                OnPropertyChangedIf(finishedChanged, nameof(IsFinished));
                OnPropertyChangedIf(openChanged, nameof(IsOpen));
            }
        }
        /// <summary>Gets the current progress state of the scan.</summary>
        public ScanProgressState ProgressState {
            get => VolatileGet(ref progressState);
            protected set => VolatileSet(ref progressState, value);
        }

        /// <summary>Gets if the scanner can guarantee some estimate of scan progress.</summary>
        public bool CanDisplayProgress {
            get => VolatileGet(ref canDisplayProgress);
            protected set => VolatileSet(ref canDisplayProgress, value);
        }
        /// <summary>Gets the progress of the scan, or 1 if the scan is ended.</summary>
        public double Progress {
            get {
                lock (volatileLock) {
                    if (canDisplayProgress) {
                        switch (progressState) {
                            case ScanProgressState.Started:
                                return (double) totalScannedSize / (totalSize - totalFreeSpace);
                            case ScanProgressState.Ending:
                            case ScanProgressState.Ended:
                                return 1d;
                        }
                    }
                    return 0d;
                }
            }
        }
        /// <summary>Gets if the scanner is scanning or cancelling a scan.</summary>
        public bool IsScanning {
            get {
                lock (volatileLock)
                    return scanState == ScanState.Scanning || scanState == ScanState.Cancelling;
            }
        }
        // Todo: We can probably remove this
        /// <summary>Gets if the scanner is scanning or cancelling a scan, but not refreshing.</summary>
        public bool IsScanningAndNotRefreshing {
            get {
                lock (volatileLock)
                    return (scanState == ScanState.Scanning || scanState == ScanState.Cancelling) &&
                            !isRefreshing;
            }
        }
        /// <summary>Gets if the scanner is refreshing.</summary>
        public bool IsRefreshing {
            get => VolatileGet(ref isRefreshing);
            protected set => VolatileSet(ref isRefreshing, value);
        }
        /// <summary>Gets if the scanner has a successfully finished scan.</summary>
        public bool IsFinished {
            get => VolatileGet(ref scanState) == ScanState.Finished;
        }
        /// <summary>Gets if a finished or cancelled scanned file tree is open.</summary>
        public bool IsOpen {
            get {
                lock (volatileLock)
                    return scanState == ScanState.Finished || scanState == ScanState.Cancelled;
            }
        }
        /// <summary>Gets or sets if the current scan operation is suspended.</summary>
        public bool IsSuspended {
            get {
                lock (resumeLock) {
                    if (!disposed)
                        return !resumeEvent.WaitOne(0);
                }
                return false;
            }
            set {
                lock (resumeLock) {
                    lock (volatileLock) {
                        //if (!IsScanning)
                        //	throw new InvalidOperationException("Cannot changed suspended state while not scanning!");
                        if (IsSuspended == value)
                            return;

                        if (!disposed) {
                            if (value)
                                resumeEvent.Reset();
                            else
                                resumeEvent.Set();
                        }
                    }
                }
                OnPropertyChanged();
            }
        }
        /// <summary>Gets if an asynchronous scan thread is running.</summary>
        public bool IsAsync {
            get {
                lock (volatileLock)
                    return scanTask != null && !scanTask.IsCompleted;
            }
        }
        /// <summary>Gets if an asynchronous scan thread is running.</summary>
        public Exception ExceptionResult {
            get => VolatileGet(ref exceptionResult);
            private set => VolatileSet(ref exceptionResult, value);
        }

        /// <summary>Gets if the UI refreshing should be supressed due to validation.</summary>
        public bool SuppressFileTreeRefresh {
            get => VolatileGet(ref suppressRefresh);
            private set => VolatileSet(ref suppressRefresh, value);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>Disposes of the scanning service.</summary>
        public async ValueTask DisposeAsync() {
            if (!disposed) {
                disposed = true;

                using (scanTask)
                using (cancel)
                using (validateTimer)
                using (resumeEvent) {
                    cancel?.Cancel();
                    await (scanTask ?? Task.CompletedTask);
                }
            }
        }

        /// <summary>Disposes of the scanning service.</summary>
        /*protected virtual void Dispose(bool disposing) {
		}*/

        #endregion

        #region Protected Properties

        /// <summary>Gets or sets the total size that has been scanned so far.</summary>
        protected long TotalScannedSize {
            get { lock (volatileLock) return totalScannedSize; }
            set { lock (volatileLock) totalScannedSize = value; }
        }
        /// <summary>Gets or sets the total size for all scanned roots.</summary>
        protected long TotalSize {
            get { lock (volatileLock) return totalSize; }
            set { lock (volatileLock) totalSize = value; }
        }
        /// <summary>Gets or sets the total free space for all scanned roots.</summary>
        protected long TotalFreeSpace {
            get { lock (volatileLock) return totalFreeSpace; }
            set { lock (volatileLock) totalFreeSpace = value; }
        }

        #endregion

        #region Protected Methods

        /// <summary>Checks if the scan operation is suspended and waits until resume if it is.</summary>
        protected bool AsyncChecks(CancellationToken token) {
            if (disposed)
                return true;

            if (!resumeEvent.WaitOne(0)) {
                StopValidateTimer();
                // Let's validate before suspension so that things are up to date
                BasicValidate();
                resumeEvent.WaitOne();
                StartValidateTimer();
                validationRequested = false;
            }
            else if (validationRequested) {
                StopValidateTimer();
                BasicValidate();
                StartValidateTimer();
                validationRequested = false;
            }
            return token.IsCancellationRequested;
        }

        /// <summary>Performs a basic validation of the file tree being scanned.</summary>
        private void BasicValidate() {
            if (RootItem != null) {
                ui.Invoke(() => {
                    Stopwatch validateWatch = Stopwatch.StartNew();
                    SuppressFileTreeRefresh = true;
                    RootItem.BasicValidate();
                    SuppressFileTreeRefresh = false;
                    TimeSpan validateTime = validateWatch.Elapsed;
                    Debug.WriteLine($"Took {validateTime.TotalMilliseconds}ms to validate");
                });
            }
        }

        #endregion

        #region Drives

        /// <summary>Scans and returns all valid drive items.</summary>
        /// 
        /// <returns>All valid drive items.</returns>
        public DriveItem[] ScanDrives() {
            List<DriveItem> drives = new List<DriveItem>();
            DriveInfo[] driveInfos = DriveInfo.GetDrives();
            for (int i = 0; i < driveInfos.Length; i++) {
                DriveInfo driveInfo = driveInfos[i];
                if (IsDriveValid(driveInfo))
                    drives.Add(new DriveItem(driveInfo));
            }
            return drives.ToArray();
        }

        /// <summary>Scans and returns all valid drive names.</summary>
        /// 
        /// <returns>All valid drive names.</returns>
        public string[] ScanDriveNames() {
            List<string> paths = new List<string>();
            DriveInfo[] driveInfos = DriveInfo.GetDrives();
            for (int i = 0; i < driveInfos.Length; i++) {
                DriveInfo driveInfo = driveInfos[i];
                if (IsDriveValid(driveInfo))
                    paths.Add(driveInfo.Name);
            }
            return paths.ToArray();
        }

        /// <summary>Gets if the drive is valid for use.</summary>
        /// 
        /// <param name="driveInfo">The drive to check.</param>
        /// <returns>True if the drive is valid.</returns>
        public bool IsDriveValid(DriveInfo driveInfo) {
            return driveInfo.IsReady &&
                    driveInfo.DriveType != DriveType.Unknown &&
                    driveInfo.DriveType != DriveType.NoRootDirectory;
        }

        #endregion

        #region Abstract Methods

        /// <summary>Runs the scan process.</summary>
        /// 
        /// <param name="rootPaths">The root paths to scan.</param>
        /// <param name="token">The scan cancellation token.</param>
        //protected abstract void Scan(string[] rootPaths, CancellationToken token);

        /// <summary>Cleanup called as a scan finishes.</summary>
        //protected abstract void FinishedCleanup();
        /// <summary>Cleanup called when a scanned file tree is closed.</summary>
        //protected abstract void ClosedCleanup();

        #endregion

        #region File Management

        /// <summary>Permanently deletes the file.</summary>
        /// 
        /// <param name="file">The path of the file to delete.</param>
        /// <returns>True if the operation was successful.</returns>
        public bool DeleteFile(FileItemBase file) {
            return DeleteOrRecycleFile(file, false);
        }

        /// <summary>Sends the file or directory to the recycle bin.</summary>
        /// 
        /// <param name="file">The path of the file to delete.</param>
        /// <returns>True if the operation was successful.</returns>
        public bool RecycleFile(FileItemBase file) {
            return DeleteOrRecycleFile(file, true);
        }

        /// <summary>Deletes or recycles the file based on the conditional value.</summary>
        /// 
        /// <param name="file">The path of the file to delete.</param>
        /// <param name="recycle">True if the file should be recycled.</param>
        /// <returns>True if the operation was successful.</returns>
        public bool DeleteOrRecycleFile(FileItemBase file, bool recycle) {
            ThrowIfScanning();
            if (!file.IsFileType || file.Type == FileItemType.Volume)
                throw new InvalidOperationException("Can only delete files or folders");
            if (os.DeleteOrRecycleFile(file.FullName, recycle)) {
                // File was successfully deleted, let's remove it from the file tree
                // Set IsRefreshing to trigger any required UI invalidation
                IsRefreshing = true;
                FolderItem parent = file.Parent;
                parent.RemoveItem(file);
                parent.UpdwardsFullValidate();
                IsRefreshing = false;
                return true;
            }
            return false;
        }

        #endregion

        #region Public Refreshing

        /// <summary>Reloads the selected files.</summary>
        /// 
        /// <param name="selectedFiles">The files to reload.</param>
        public void RefreshFilesAsync(IEnumerable<FileItemBase> selectedFiles) {
            if (!IsOpen)
                throw new InvalidOperationException("No scan is open to refresh!");
            ThrowIfScanning();
            lock (threadLock) {
                ScanPrepare(true);
                Debug.Assert(scanTask == null);
                scanTask = Task.Run(() => RefreshThread(selectedFiles, cancel.Token));
            }
        }

        private void RefreshThread(IEnumerable<FileItemBase> selectedFiles, CancellationToken token) {
            FileItemBase[] isolatedFiles = FolderItem.IsolateAncestores(selectedFiles);
            if (isolatedFiles.Length > 0) {
                if (isolatedFiles[0].IsAbsoluteRootType) {
                    // Just perform a full reload
                    ScanThread(false, token);
                    return;
                }

                // Group all files into lists for each similar parent
                Dictionary<FolderItem, List<FileItemBase>> refreshFilesMap = new Dictionary<FolderItem, List<FileItemBase>>();
                for (int i = 0; i < isolatedFiles.Length; i++) {
                    FileItemBase file = isolatedFiles[i];
                    FolderItem parent = file.FileParent;
                    if (!refreshFilesMap.TryGetValue(parent, out List<FileItemBase> parentFiles)) {
                        parentFiles = new List<FileItemBase>();
                        refreshFilesMap.Add(parent, parentFiles);
                    }
                    parentFiles.Add(file);
                }

                // Convert the refresh files to an array
                refreshFiles = new RefreshFiles[refreshFilesMap.Count];
                int index = 0;
                foreach (var pair in refreshFilesMap)
                    refreshFiles[index++] = new RefreshFiles(pair);

                // Next remove the files' children from the tree
                foreach (RefreshFiles parentFile in refreshFiles) {
                    FolderItem parent = parentFile.Parent;
                    FolderItem fileCollection = parent.GetFileCollection();
                    int count = parentFile.Files.Count;
                    for (int i = 0; i < count; i++) {
                        if (parentFile.Files[i] is FolderItem folder)
                            folder.ClearItems();
                    }
                }

                // Measures to ensure garbage collection
                selectedFiles = null;
                isolatedFiles = null;
                refreshFilesMap = null;

                if (AsyncChecks(token))
                    return;

                ScanThread(true, token);
            }
        }

        /// <summary>Reloads the scanned file tree from the beginning.</summary>
        public void ReloadAsync() {
            if (!IsOpen)
                throw new InvalidOperationException("No scan is open to reload!");
            ThrowIfScanning();
            lock (threadLock) {
                ScanPrepare(true);
                rootPaths = driveSelectResult.GetResultPaths();
                Debug.Assert(scanTask == null);
                scanTask = Task.Run(() => ScanThread(false, cancel.Token));
            }
        }

        #endregion
    }
}
