using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Services {
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
