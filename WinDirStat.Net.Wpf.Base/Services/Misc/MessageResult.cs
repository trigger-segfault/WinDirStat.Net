using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Services {
	/// <summary>Specifies result from a dialog message box.</summary>
	public enum MessageResult {
		/// <summary>The message box returns no result.</summary>
		None = 0,
		/// <summary>The result value of the message box is OK.</summary>
		OK = 1,
		/// <summary>The result value of the message box is Cancel.</summary>
		Cancel = 2,
		/// <summary>The result value of the message box is Yes.</summary>
		Yes = 6,
		/// <summary>The result value of the message box is No.</summary>
		No = 7,
	}

	/// <summary>Specifies the buttons that are displayed on a dialog message.</summary>
	public enum MessageButton {
		/// <summary>The message box displays an OK button.</summary>
		OK = 0,
		/// <summary>The message box displays OK and Cancel buttons.</summary>
		OKCancel = 1,
		/// <summary>The message box displays Yes and No buttons.</summary>
		YesNo = 4,
		/// <summary>The message box displays Yes, No, and Cancel buttons.</summary>
		YesNoCancel = 3,
	}
}
