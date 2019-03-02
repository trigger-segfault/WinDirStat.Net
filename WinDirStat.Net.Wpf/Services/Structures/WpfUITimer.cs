using System;
using System.Windows;
using System.Windows.Threading;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Wpf.Services.Structures {
	/// <summary>A timer that runs its callbacks on the UI thread.</summary>
	public class WpfUITimer : IUITimer {

		#region Fields

		/// <summary>The wrapped dispatcher timer.</summary>
		private readonly DispatcherTimer dispatcherTimer;
		/// <summary>The event handler callback constructed from the action.</summary>
		private EventHandler handlerCallback;
		/// <summary>The action used for the callback.</summary>
		private Action callback;

		#endregion

		#region Constructors

		/// <summary>Constructs the see <see cref="WpfUITimer"/>.</summary>
		/// 
		/// <param name="interval">The interval for the timer.</param>
		/// <param name="normalPriority">True if the timer runs at normal priority.</param>
		/// <param name="callback">The callback on the timer tick event.</param>
		/// <param name="start">True if the timer should start running.</param>
		public WpfUITimer(TimeSpan interval, bool normalPriority, Action callback, bool start) {
			this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
			handlerCallback = (o, s) => callback();
			dispatcherTimer = new DispatcherTimer(
				interval,
				WpfUIService.GetPriority(normalPriority),
				handlerCallback,
				Application.Current.Dispatcher);
			if (!start)
				dispatcherTimer.Stop();
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets the callback method for the timer.</summary>
		public Action Callback {
			get => callback;
			set {
				if (callback != value) {
					callback = value ?? throw new ArgumentNullException(nameof(Callback));
					dispatcherTimer.Tick -= handlerCallback;
					handlerCallback = (o, s) => callback();
					dispatcherTimer.Tick += handlerCallback;
				}
			}
		}
		/// <summary>Gets or sets the interval for the timer.</summary>
		public TimeSpan Interval {
			get => dispatcherTimer.Interval;
			set => dispatcherTimer.Interval = value;
		}
		/// <summary>Gets or sets if the timer is running.</summary>
		public bool IsRunning {
			get => dispatcherTimer.IsEnabled;
			set {
				if (dispatcherTimer.IsEnabled != value) {
					if (value)
						dispatcherTimer.Start();
					else
						dispatcherTimer.Stop();
				}
			}
		}

		#endregion

		#region Start/Stop

		/// <summary>Starts the timer.</summary>
		public void Start() {
			dispatcherTimer.Start();
		}
		/// <summary>Stops the timer.</summary>
		public void Stop() {
			dispatcherTimer.Stop();
		}
		/// <summary>Restarts the timer.</summary>
		public void Restart() {
			dispatcherTimer.Stop();
			dispatcherTimer.Start();
		}

		#endregion
	}
}
