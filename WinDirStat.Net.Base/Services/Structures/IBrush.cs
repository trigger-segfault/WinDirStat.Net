using System;
using System.Collections.Generic;
using System.Text;

namespace WinDirStat.Net.Services.Structures {
	/// <summary>An interface for a UI-independent brush.</summary>
	public interface IBrush {
		
		/// <summary>Gets the actual brush object.</summary>
		object Brush { get; }
	}
}
