using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Services {
	/// <summary>A service for interacting with the clipboard.</summary>
	public interface IClipboardService {

		/// <summary>Gets the text in the clipboad.</summary>
		string GetText();
		/// <summary>Sets the text in the clipboad.</summary>
		void SetText(string text);

	}
}
