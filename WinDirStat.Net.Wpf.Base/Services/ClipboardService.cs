using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.Services {
	/// <summary>The service for managing the clipboard state.</summary>
	public class ClipboardService {
		/// <summary>Gets the text assigned to the clipboard.</summary>
		/// <returns>A string if the clipboard has text assigned to it.</returns>
		public string GetText() {
			return Clipboard.GetText();
		}

		/// <summary>Assigns text to the clipboard.</summary>
		/// 
		/// <param name="text">The new text to assign to the clipboard.</param>
		public void SetText(string text) {
			Clipboard.SetText(text);
		}
	}
}
