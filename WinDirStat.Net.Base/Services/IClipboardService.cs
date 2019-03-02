using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Services {
	/// <summary>A service for interacting with the clipboard.</summary>
	public interface IClipboardService {

		#region Text

		/// <summary>Gets the text assigned to the clipboard.</summary>
		/// 
		/// <returns>A string if the clipboard has text assigned to it.</returns>
		string GetText();
		/// <summary>Assigns text to the clipboard.</summary>
		/// 
		/// <param name="text">The new text to assign to the clipboard.</param>
		void SetText(string text);

		#endregion
	}
}
