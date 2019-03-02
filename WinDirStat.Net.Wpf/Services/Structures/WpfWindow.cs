using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using WinDirStat.Net.Services.Structures;

namespace WinDirStat.Net.Wpf.Services.Structures {
	/// <summary>A WPF implementation of <see cref="IWindow"/>.</summary>
	public class WpfWindow : IWindow {

		#region Fields

		/// <summary>Gets the actual Wpf window.</summary>
		public Window Window { get; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="WpfWindow"/> from a <see cref="Window"/>.</summary>
		public WpfWindow(Window window) {
			Window = window ?? throw new ArgumentNullException(nameof(window));
		}

		#endregion

		#region Properties
		
		/// <summary>Gets the window that owns this window.</summary>
		public IWindow Owner {
			get => Invoke(() => (Window.Owner != null ? new WpfWindow(Window.Owner) : null));
		}
		/// <summary>Gets the children owned by this window.</summary>
		IWindow[] IWindow.Children {
			get => Invoke(() => Window.OwnedWindows.Cast<Window>().Select(w => (IWindow) new WpfWindow(w)).ToArray());
		}
		/// <summary>Gets or sets the dialog result.</summary>
		public bool DialogResult {
			get => Invoke(() => Window.DialogResult.HasValue && Window.DialogResult.Value);
			set => Invoke(() => Window.DialogResult = value);
		}
		/// <summary>Gets the handle for the window.</summary>
		public IntPtr Handle => Invoke(() => new WindowInteropHelper(Window).Handle);
		/// <summary>Gets the actual Wpf window.</summary>
		object IWindow.Window => Window;

		#endregion

		#region Private Helpers

		/// <summary>Invokes the action on the dispatcher thread.</summary>
		private void Invoke(Action callback) {
			Window.Dispatcher.Invoke(callback);
		}

		/// <summary>Gets the values from the dispatcher thread.</summary>
		private T Invoke<T>(Func<T> callback) {
			return Window.Dispatcher.Invoke(callback);
		}

		#endregion

		#region Close

		/// <summary>Closes the window.</summary>
		public void Close() {
			Invoke(Window.Close);
		}

		/// <summary>Called when the window is closed.</summary>
		public event EventHandler Closed {
			add => Invoke(() => Window.Closed += value);
			remove => Invoke(() => Window.Closed -= value);
		}

		#endregion

		#region Equals

		/// <summary>Gets if the two windows are referencing the same window.</summary>
		/// 
		/// <param name="window">The window wrapper to compare.</param>
		/// <returns>True if the interfaces are referencing the same window.</returns>
		public bool Equals(IWindow window) {
			return Window == window?.Window;
		}

		#endregion
	}
}
