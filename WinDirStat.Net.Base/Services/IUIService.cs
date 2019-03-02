using System;
using WinDirStat.Net.Services.Structures;

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
}
