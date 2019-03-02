using System.Windows;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.Wpf.Services {
	/// <summary>The service for managing the clipboard state.</summary>
	public class WpfClipboardService  : IClipboardService {

		#region Text

		/// <summary>Gets the text assigned to the clipboard.</summary>
		/// 
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

		#endregion
	}
}
