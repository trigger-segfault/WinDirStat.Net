using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Model.Files;

namespace WinDirStat.Net.Services {
	

	

	/// <summary>A service for scanning a file tree.</summary>
	public interface IScanningService : INotifyPropertyChanged, IDisposable {



		/// <summary>The available root item once the scan has fully started.</summary>
		RootItem RootItem { get; }
		

		event ScanEventHander Started;
		event ScanEventHander Ended;
		event EventHandler SpaceChanged;

		bool ShowFreeSpace { get; }
		bool ShowUnknown { get; }
		bool ShowTotalSpace { get; }

		ExtensionItems Extensions { get; }

		void Scan(params string[] rootPaths);
		void ScanAsync(params string[] rootPaths);
		void Close(bool waitForClose = false);
		void CloseAsync(Action callback, bool runWhenAlreadyClosed = true);
		void Cancel(bool waitForCancel = false);
		void CancelAsync(Action callback, bool runWhenNotScanning = true);
		void CancelAsync(ScanEventHander callback, bool runWhenNotScanning = true);

		TimeSpan ScanTime { get; }
		TimeSpan ValidateTime { get; }
		ScanState ScanState { get; }
		ScanProgressState ProgressState { get; }

		bool CanDisplayProgress { get; }
		double Progress { get; }

		bool IsScanning { get; }
		bool IsScanningAndNotRefreshing { get; }
		bool IsRefreshing { get; }
		bool IsFinished { get; }
		bool IsOpen { get; }
		bool IsSuspended { get; set; }
		bool IsAsync { get; }
		Exception ExceptionResult { get; }
	}
}
