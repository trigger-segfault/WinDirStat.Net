using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Data;

namespace WinDirStat.Net.Model.View {
	partial class WinDirStatViewModel {
		public TimeSpan ScanTime {
			get => model.ScanTime;
		}
		public ScanState ScanState {
			get => model.ScanState;
		}
		public ScanProgressState ProgressState {
			get => model.ProgressState;
		}
		public bool CanDisplayProgress {
			get => model.CanDisplayProgress;
		}
		public double Progress {
			get => model.Progress;
			private set {
				if (progress != value) {
					progress = value;
					AutoRaisePropertyChanged();
				}
			}
		}
		public bool IsScanning {
			get => model.IsScanning;
		}
		public bool IsScanningAndNotRefreshing {
			get => model.IsScanningAndNotRefreshing;
		}
		public bool IsRefreshing {
			get => model.IsRefreshing;
			private set => model.IsRefreshing = value;
		}
		public bool IsFinished {
			get => model.IsFinished;
		}
		public bool IsSuspended {
			get => model.IsSuspended;
			set => model.IsSuspended = value;
		}

		public bool IsAsync {
			get => model.IsAsync;
		}
	}
}
