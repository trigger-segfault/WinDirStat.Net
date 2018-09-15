using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Services {
	/// <summary>A service for all UI actions.</summary>
	public interface IUIService {

		#region Dispatcher

		/// <summary>Invokes the action on the UI thread.</summary>
		/// 
		/// <param name="action">The action to invoke.</param>
		void Invoke(Action action);

		/// <summary>Invokes the function on the UI thread.</summary>
		/// 
		/// <param name="action">The function to invoke.</param>
		/// <returns>The result of the function.</returns>
		T Invoke<T>(Func<T> action);

		/// <summary>Invokes the action asynchronously on the UI thread.</summary>
		/// 
		/// <param name="action">The action to invoke.</param>
		/// <param name="normalPriority">True if the action should use normal priority.</param>
		void BeginInvoke(Action action, bool normalPriority);

		/// <summary>Checks if the current thread is the UI thread.</summary>
		/// 
		/// <returns>True if the current thread is the UI thread.</returns>
		bool CheckAccess();

		#endregion

		#region Shutdown

		/// <summary>Shuts down the application.</summary>
		void Shutdown();

		event EventHandler ShuttingDown;

		#endregion

		#region Create Timer

		/// <summary>Creates a new stopped UI timer.</summary>
		/// 
		/// <param name="interval">The interval for the timer.</param>
		/// <param name="normalPriority">True if the timer runs at normal priority.</param>
		/// <param name="callback">The callback on the timer tick event.</param>
		IUITimer CreateTimer(TimeSpan interval, bool normalPriority, Action callback);
		/// <summary>Creates a new running UI timer.</summary>
		/// 
		/// <param name="interval">The interval for the timer.</param>
		/// <param name="normalPriority">True if the timer runs at normal priority.</param>
		/// <param name="callback">The callback on the timer tick event.</param>
		/// <returns>The newly created timer.</returns>
		IUITimer StartTimer(TimeSpan interval, bool normalPriority, Action callback);

		#endregion
	}

	/// <summary>An interface for a UI timer.</summary>
	public interface IUITimer {

		/// <summary>Gets or sets the callback method for the timer.</summary>
		Action Callback { get; set; }
		/// <summary>Gets or sets the interval for the timer.</summary>
		TimeSpan Interval { get; set; }
		/// <summary>Gets or sets if the timer is running.</summary>
		bool IsRunning { get; set; }

		/// <summary>Starts the timer.</summary>
		void Start();
		/// <summary>Stops the timer.</summary>
		void Stop();
		/// <summary>Restarts the timer.</summary>
		void Restart();
	}
}
