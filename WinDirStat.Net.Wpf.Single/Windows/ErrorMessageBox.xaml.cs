using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using WinDirStat.Net.Utils;

namespace WinDirStat.Net.Wpf.Windows {
	/// <summary>Shows an error that occured in the program.</summary>
	public partial class ErrorMessageBox : Window {

		#region Settings

		/// <summary>Gets or sets the name of the program to show in the window.</summary>
		public static string ProgramName { get; set; } =
			Assembly.GetEntryAssembly().GetName().Name;

		/// <summary>Gets or sets the error icon to show for this window and the
		/// taskbar.</summary>
		public static ImageSource ErrorIcon { get; set; }

		/// <summary>The Uri that the custom hyperlink leads to.</summary>
		public static Uri HyperlinkUri { get; set; }
		/// <summary>The label for the custom hyperlink.</summary>
		public static string HyperlinkName { get; set; }

		#endregion

		#region Static Fields

		/// <summary>The last exception. Used to prevent multiple error windows for the same error.</summary>
		private static object lastException;
		/// <summary>True if an exception window is open.</summary>
		private static bool exceptionOpen;

		#endregion

		#region Fields

		/// <summary>The exception object that was raised.</summary>
		private object exception;
		/// <summary>True if viewing the full exception.</summary>
		private bool viewingFull;
		/// <summary>The timer for changing the copy button back to its original text.</summary>
		private DispatcherTimer copyTimer;

		#endregion

		#region Constructors

		/// <summary>Constructs the error message box with an exception object.</summary>
		private ErrorMessageBox(object ex, bool alwaysContinue) {
			InitializeComponent();

			viewingFull = false;
			textBlockMessage.Text = "Exception:\n";
			exception = ex;
			if (IsException)
				textBlockMessage.Text += Exception.MessageWithInner();
			else
				textBlockMessage.Text += exception.ToString();
			if (!IsException) {
				buttonException.IsEnabled = false;
			}
			if (alwaysContinue) {
				buttonExit.Visibility = Visibility.Collapsed;
				buttonContinue.IsDefault = true;
			}

			// Apply static settings
			textBlockName.Text = $"Unhandled Exception in {ProgramName}!";
			Icon = ErrorIcon;

			if (!string.IsNullOrWhiteSpace(HyperlinkName) && HyperlinkUri != null) {
				hyperlink.Inlines.Clear();
				hyperlink.Inlines.Add(HyperlinkName);
				hyperlink.NavigateUri = HyperlinkUri;
			}
			else {
				labelHyperlink.Visibility = Visibility.Collapsed;
			}

			copyTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, (o, e) => {
				buttonCopy.Content = "Copy to Clipboard";
			}, Dispatcher);
			copyTimer.Stop();
		}

		#endregion

		#region Event Handlers

		private void OnWindowLoaded(object sender, RoutedEventArgs e) {
			// Only play a noisy sound if this window was not prompted by the user
			if (buttonExit.Visibility == Visibility.Visible) {
				SystemSounds.Hand.Play();
			}
		}

		private void OnExit(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}

		private void OnCopyToClipboard(object sender, RoutedEventArgs e) {
			copyTimer.Stop();
			Clipboard.SetText(exception.ToString());
			buttonCopy.Content = "Exception Copied!";
			copyTimer.Start();
		}

		private void OnSeeFullException(object sender, RoutedEventArgs e) {
			viewingFull = !viewingFull;
			if (!viewingFull) {
				buttonException.Content = "See Full Exception";
				textBlockMessage.Text = $"Exception:\n{Exception.MessageWithInner()}";
				clientArea.Height = 230;
				scrollViewer.ScrollToTop();
			}
			else {
				buttonException.Content = "Hide Full Exception";
				// Size may not be changed yet so just
				// incase we also have OnMessageSizeChanged.
				textBlockMessage.Text = $"Exception:\n{Exception.ToStringWithInner()}";
				UpdateFullMessageSize();
			}
		}

		private void OnMessageSizeChanged(object sender, SizeChangedEventArgs e) {
			if (viewingFull)
				UpdateFullMessageSize();
		}

		private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
			var focused = FocusManager.GetFocusedElement(this);
			switch (e.Key) {
			case Key.Right:
				if (focused == buttonContinue && buttonExit.Visibility == Visibility.Visible)
					buttonExit.Focus();
				else if (focused == buttonCopy)
					buttonContinue.Focus();
				else if (focused == buttonException)
					buttonCopy.Focus();
				e.Handled = true;
				break;
			case Key.Left:
				if (focused == null) {
					if (buttonExit.Visibility == Visibility.Visible)
						buttonContinue.Focus();
					else
						buttonCopy.Focus();
				}
				else if (focused == buttonExit)
					buttonContinue.Focus();
				else if (focused == buttonContinue)
					buttonCopy.Focus();
				else if (focused == buttonCopy && buttonException.IsEnabled)
					buttonException.Focus();
				e.Handled = true;
				break;
			}
		}

		private void OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
			Process.Start((sender as Hyperlink).NavigateUri.ToString());
		}

		#endregion

		#region Private Methods
		
		private void UpdateFullMessageSize() {
			clientArea.Height = Math.Max(230, Math.Min(480,
				textBlockMessage.ActualHeight + 102));
			scrollViewer.ScrollToTop();
		}

		#endregion

		#region Show

		/// <summary>Shows an error message box with an exception object.</summary>
		public static bool Show(object exception, bool alwaysContinue = false) {
			ErrorMessageBox messageBox = new ErrorMessageBox(exception, alwaysContinue);
			var result = messageBox.ShowDialog();
			return result.HasValue && result.Value;
		}

		#endregion

		#region GlobalHook

		/// <summary>Hook the error message box to catch all exceptions.</summary>
		public static void GlobalHook(Application app) {
			app.DispatcherUnhandledException +=
				OnDispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException +=
				OnAppDomainUnhandledException;
			TaskScheduler.UnobservedTaskException +=
				OnTaskSchedulerUnobservedTaskException;
			app.Exit += OnAppShutdown;
		}

		#endregion

		#region Private Hooking

		/// <summary>Unhooks all exception catching.</summary>
		private static void OnAppShutdown(object sender, ExitEventArgs e) {
			(sender as Application).DispatcherUnhandledException -=
				OnDispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException -=
				OnAppDomainUnhandledException;
			TaskScheduler.UnobservedTaskException -=
				OnTaskSchedulerUnobservedTaskException;
		}

		/// <summary>Show an exception window for an exception that occurred in a
		/// dispatcher thread.</summary>
		private static void OnDispatcherUnhandledException(object sender,
			DispatcherUnhandledExceptionEventArgs e)
		{
			ShowErrorMessageBox(e.Exception);
			e.Handled = true;
		}

		/// <summary>Show an exception window for an exception that occurred in the
		/// AppDomain.</summary>
		private static void OnAppDomainUnhandledException(object sender,
			UnhandledExceptionEventArgs e)
		{
			Application.Current.Dispatcher.Invoke(
				() => ShowErrorMessageBox(e.ExceptionObject));
		}

		/// <summary>Show an exception window for an exception that occurred in a task.</summary>
		private static void OnTaskSchedulerUnobservedTaskException(object sender,
			UnobservedTaskExceptionEventArgs e)
		{
			Application.Current.Dispatcher.Invoke(
				() => ShowErrorMessageBox(e.Exception));
		}

		/// <summary>The helper method for showing an error message.</summary>
		private static void ShowErrorMessageBox(object ex) {
			// Ignore these thrown by the debugger
			if (ex is TaskCanceledException)
				return;
			if (ex != lastException && !exceptionOpen) {
				//TimerEvents.PauseAll();
				lastException = ex;
				exceptionOpen = true;
				if (Show(ex))
					Environment.Exit(0);
				exceptionOpen = false;
				//TimerEvents.ResumeAll();
			}
		}

		#endregion

		#region Properties

		/// <summary>Gets if the exception object is an exception.</summary>
		public bool IsException =>exception is Exception;

		/// <summary>Gets the exception object as an exception.</summary>
		public Exception Exception => exception as Exception;

		#endregion

		#region Static Properties

		/// <summary>Gets if the exception window is open.</summary>
		public static bool IsExceptionOpen => exceptionOpen;

		#endregion
	}
}
