using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WinDirStat.Net.Services;
using WinDirStat.Net.Services.Structures;
using WinDirStat.Net.Wpf.Services.Structures;

namespace WinDirStat.Net.Wpf.Services {
	/// <summary>A service for UI interactions.</summary>
	public class WpfUIService : IUIService {

		#region Fields

		/// <summary>The UI dispatcher.</summary>
		public Dispatcher Dispatcher { get; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="WpfUIService"/>.</summary>
		public WpfUIService() {
			Dispatcher = Application.Current.Dispatcher;
		}

		#endregion

		#region Dispatcher

		/// <summary>Invokes the action on the UI thread.</summary>
		/// 
		/// <param name="action">The action to invoke.</param>
		public void Invoke(Action action) {
			try {
				Dispatcher.Invoke(action);
			}
			catch (TaskCanceledException) { }
		}

		/// <summary>Invokes the function on the UI thread.</summary>
		/// 
		/// <param name="action">The function to invoke.</param>
		/// <returns>The result of the function.</returns>
		public T Invoke<T>(Func<T> action) {
			return Dispatcher.Invoke(action);
		}

		/// <summary>Invokes the action asynchronously on the UI thread.</summary>
		/// 
		/// <param name="action">The action to invoke.</param>
		/// <param name="normalPriority">True if the action should use normal priority.</param>
		public void BeginInvoke(Action action, bool normalPriority) {
			Dispatcher.BeginInvoke(action, GetPriority(normalPriority));
		}

		/// <summary>Checks if the current thread is the UI thread.</summary>
		/// 
		/// <returns>True if the current thread is the UI thread.</returns>
		public bool CheckAccess() {
			return Dispatcher.CheckAccess();
		}

		#endregion

		#region Shutdown

		/// <summary>Shuts down the application.</summary>
		public void Shutdown() {
			Dispatcher.InvokeShutdown();
		}

		public event EventHandler ShuttingDown {
			add => Dispatcher.ShutdownStarted += value;
			remove => Dispatcher.ShutdownStarted -= value;
		}

		#endregion

		#region Create Timer

		/// <summary>Creates a new stopped UI timer.</summary>
		/// 
		/// <param name="interval">The interval for the timer.</param>
		/// <param name="normalPriority">True if the timer runs at normal priority.</param>
		/// <param name="callback">The callback on the timer tick event.</param>
		/// <returns>The newly created timer.</returns>
		public IUITimer CreateTimer(TimeSpan interval, bool normalPriority, Action callback) {
			return new WpfUITimer(interval, normalPriority, callback, false);
		}
		/// <summary>Creates a new running UI timer.</summary>
		/// 
		/// <param name="interval">The interval for the timer.</param>
		/// <param name="normalPriority">True if the timer runs at normal priority.</param>
		/// <param name="callback">The callback on the timer tick event.</param>
		/// <returns>The newly created timer.</returns>
		public IUITimer StartTimer(TimeSpan interval, bool normalPriority, Action callback) {
			return new WpfUITimer(interval, normalPriority, callback, true);
		}

		#endregion

		#region Static Helpers

		/// <summary>Gets the priorty as a <see cref="DispatcherPriority"/>.</summary>
		public static DispatcherPriority GetPriority(bool normalPriority) {
			return (normalPriority ? DispatcherPriority.Normal : DispatcherPriority.Background);
		}

		#endregion
	}
}
