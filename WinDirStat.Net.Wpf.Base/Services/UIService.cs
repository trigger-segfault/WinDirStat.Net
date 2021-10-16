using System;
using System.Windows;
using System.Windows.Threading;

namespace WinDirStat.Net.Services {
    /// <summary>A service for UI interactions.</summary>
    public class UIService {

        #region Fields

        /// <summary>The UI dispatcher.</summary>
        public Dispatcher Dispatcher { get; }

        #endregion

        #region Constructors

        /// <summary>Constructs the <see cref="UIService"/>.</summary>
        public UIService() {
            Dispatcher = Application.Current.Dispatcher;
        }

        #endregion

        #region Dispatcher

        /// <summary>Invokes the action on the UI thread.</summary>
        /// 
        /// <param name="action">The action to invoke.</param>
        public void Invoke(Action action) => Dispatcher.Invoke(action);

        /// <summary>Invokes the function on the UI thread.</summary>
        /// 
        /// <param name="action">The function to invoke.</param>
        /// <returns>The result of the function.</returns>
        public T Invoke<T>(Func<T> action) => Dispatcher.Invoke(action);

        /// <summary>Invokes the action asynchronously on the UI thread.</summary>
        /// 
        /// <param name="action">The action to invoke.</param>
        /// <param name="normalPriority">True if the action should use normal priority.</param>
        public void BeginInvoke(Action action, bool normalPriority) => Dispatcher.BeginInvoke(action, GetPriority(normalPriority));

        /// <summary>Checks if the current thread is the UI thread.</summary>
        /// 
        /// <returns>True if the current thread is the UI thread.</returns>
        public bool CheckAccess() => Dispatcher.CheckAccess();

        #endregion

        #region Shutdown

        /// <summary>Shuts down the application.</summary>
        public void Shutdown() => Dispatcher.InvokeShutdown();

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
        public UITimer CreateTimer(TimeSpan interval, bool normalPriority, Action callback) => new UITimer(interval, normalPriority, callback, false);
        /// <summary>Creates a new running UI timer.</summary>
        /// 
        /// <param name="interval">The interval for the timer.</param>
        /// <param name="normalPriority">True if the timer runs at normal priority.</param>
        /// <param name="callback">The callback on the timer tick event.</param>
        /// <returns>The newly created timer.</returns>
        public UITimer StartTimer(TimeSpan interval, bool normalPriority, Action callback) => new UITimer(interval, normalPriority, callback, true);

        #endregion

        public static DispatcherPriority GetPriority(bool normalPriority) => normalPriority ? DispatcherPriority.Normal : DispatcherPriority.Background;
    }

    /// <summary>A timer that runs its callbacks on the UI thread.</summary>
    public class UITimer {

        #region Fields

        /// <summary>The wrapped dispatcher timer.</summary>
        private readonly DispatcherTimer dispatcherTimer;
        /// <summary>The event handler callback constructed from the action.</summary>
        private EventHandler handlerCallback;
        /// <summary>The action used for the callback.</summary>
        private Action callback;

        #endregion

        #region Constructors

        /// <summary>Constructs the see <see cref="UITimer"/>.</summary>
        /// 
        /// <param name="interval">The interval for the timer.</param>
        /// <param name="normalPriority">True if the timer runs at normal priority.</param>
        /// <param name="callback">The callback on the timer tick event.</param>
        /// <param name="start">True if the timer should start running.</param>
        public UITimer(TimeSpan interval, bool normalPriority, Action callback, bool start) {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            handlerCallback = (o, s) => callback();
            dispatcherTimer = new DispatcherTimer(
                interval,
                UIService.GetPriority(normalPriority),
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
