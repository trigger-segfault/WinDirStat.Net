using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Data {
	[Serializable]
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

	[Serializable]
	public enum ScanProgressState {
		NotStarted,
		Starting,
		Started,
		Ending,
		Ended,
	}
	
	public class ScanEventArgs {
		public ScanState ScanState { get; }
		public ScanProgressState ProgressState { get; }
		public Exception Exception { get; }

		public ScanEventArgs(ScanState state, ScanProgressState progress) {
			ScanState = state;
			ProgressState = progress;
			Exception = null;
		}

		public ScanEventArgs(ScanState state, ScanProgressState progress, Exception exception) {
			ScanState = state;
			ProgressState = progress;
			Exception = exception;
		}
	}

	public delegate void ScanEventHander(object sender, ScanEventArgs e);
}
