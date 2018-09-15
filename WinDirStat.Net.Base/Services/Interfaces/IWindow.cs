using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Services {
	/// <summary>An interface for storing the window.</summary>
	public interface IWindow {

		#region Properties

		/// <summary>Gets the actual window object.</summary>
		object Window { get; }
		/// <summary>Gets the window that owns this window.</summary>
		IWindow Owner { get; }
		/// <summary>Gets the children owned by this window.</summary>
		IWindow[] Children { get; }
		/// <summary>Gets the handle for the window.</summary>
		IntPtr Handle { get; }
		/// <summary>Gets or sets the dialog result.</summary>
		bool DialogResult { get; set; }

		#endregion

		#region Close

		/// <summary>Closes the window.</summary>
		void Close();

		/// <summary>Called when the window is closed.</summary>
		event EventHandler Closed;

		#endregion

		#region Equals

		/// <summary>Gets if the two windows are referencing the same window.</summary>
		/// 
		/// <param name="window">The window wrapper to compare.</param>
		/// <returns>True if the interfaces are referencing the same window.</returns>
		bool Equals(IWindow window);

		#endregion
	}
}
