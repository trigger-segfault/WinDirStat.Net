using System;

namespace WinDirStat.Net.Services.Structures {
	/// <summary>An interface for a UI timer.</summary>
	public interface IUITimer {

		#region Properties

		/// <summary>Gets or sets the callback method for the timer.</summary>
		Action Callback { get; set; }
		/// <summary>Gets or sets the interval for the timer.</summary>
		TimeSpan Interval { get; set; }
		/// <summary>Gets or sets if the timer is running.</summary>
		bool IsRunning { get; set; }

		#endregion

		#region Start/Stop

		/// <summary>Starts the timer.</summary>
		void Start();
		/// <summary>Stops the timer.</summary>
		void Stop();
		/// <summary>Restarts the timer.</summary>
		void Restart();

		#endregion
	}
}
