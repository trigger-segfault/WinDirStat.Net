using System;
using System.Collections.Generic;
using System.Text;

namespace WinDirStat.Net.Services {
	/// <summary>An interface for a shortcut implementation.</summary>
	public interface IShortcut {

		#region Properties

		/// <summary>The actual shortcut object.</summary>
		object Shortcut { get; }

		#endregion
	}
}
