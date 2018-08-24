using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Data.Extensions;
using WinDirStat.Net.Model.Data.Nodes;
using WinDirStat.Net.Model.Settings;

namespace WinDirStat.Net.Model.Data {
	public partial class WinDirStatModel : ObservableObject, IDisposable {

		// Scanning
		private List<ScanningState> scanningStates;
		//private bool isScanAllDrives;
		private long totalScannedSize;
		private long scanTotalSize;
		private long scanTotalFreeSpace;

		private RootNode scanningRootNode;
		private RootNode rootNode;
		private AutoResetEvent resumeEvent = new AutoResetEvent(false);
		private volatile bool isScanSuspended;
		
		private readonly ExtensionRecords extensions;
		private bool disposed;
		private bool showFreeSpace;
		private bool showUnknown;


		public WinDirStatModel() {
			extensions = new ExtensionRecords(this);
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

		public bool ShowFreeSpace {
			get => showFreeSpace;
			set {
				if (showFreeSpace != value) {
					showFreeSpace = value;
					rootNode?.OnShowFreeSpaceChanged(value);
					AutoRaisePropertyChanged();
				}
			}
		}
		public bool ShowUnknown {
			get => showUnknown;
			set {
				if (showUnknown != value) {
					showUnknown = value;
					rootNode?.OnShowUnknownChanged(value);
					AutoRaisePropertyChanged();
				}
			}
		}

		public ExtensionRecords Extensions {
			get => extensions;
		}

		public void Dispose() {
			if (!disposed) {
				disposed = true;
				Cancel(false);
				resumeEvent.Dispose();
			}
		}
	}
}
