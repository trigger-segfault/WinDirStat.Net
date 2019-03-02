using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Services.Structures {
	/// <summary>The scan states for <see cref="IScanningService"/>.</summary>
	[Serializable]
	public enum ScanState {
		/// <summary>The scan has not started, or the last scan has been closed.</summary>
		NotStarted,
		/// <summary>A scan is currently in progress.</summary>
		Scanning,
		/// <summary>The current scan is suspended.</summary>
		Suspended,
		/// <summary>The last scan has finished and is still open.</summary>
		Finished,
		/// <summary>The scan is being canceled and has not ended yet.</summary>
		Cancelling,
		/// <summary>The last scan was cancelled.</summary>
		Cancelled,
		/// <summary>The last scan failed.</summary>
		Failed,
	}

	/// <summary>The scan progress states for <see cref="IScanningService"/>.</summary>
	[Serializable]
	public enum ScanProgressState {
		/// <summary>The scan has not started, or the last scan has been closed.</summary>
		NotStarted,
		/// <summary>The scan is preparing to start.</summary>
		Starting,
		/// <summary>The scan has fully started.</summary>
		Started,
		/// <summary>The scan is preparing to end.</summary>
		Ending,
		/// <summary>The scan has fully ended.</summary>
		Ended,
	}

	/// <summary>Event args for a <see cref="IScanningService"/> event.</summary>
	public class ScanEventArgs {
		/// <summary>The current scan state.</summary>
		public ScanState ScanState { get; }
		/// <summary>The current scan progress state.</summary>
		public ScanProgressState ProgressState { get; }
		/// <summary>The exception that occurred during a failed scan.</summary>
		public Exception Exception { get; }

		/// <summary>Constructs the <see cref="ScanEventArgs"/> without an exception.</summary>
		public ScanEventArgs(ScanState state, ScanProgressState progress) {
			ScanState = state;
			ProgressState = progress;
			Exception = null;
		}

		/// <summary>Constructs the <see cref="ScanEventArgs"/> with an exception.</summary>
		public ScanEventArgs(ScanState state, ScanProgressState progress, Exception exception) {
			ScanState = state;
			ProgressState = progress;
			Exception = exception;
		}
	}

	/// <summary>Event handler for a <see cref="IScanningService"/> event.</summary>
	public delegate void ScanEventHander(object sender, ScanEventArgs e);
}
