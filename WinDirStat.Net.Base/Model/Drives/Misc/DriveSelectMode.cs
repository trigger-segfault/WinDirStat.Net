using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Drives {
	/// <summary>The selection mode for drives.</summary>
	[Serializable]
	public enum DriveSelectMode {
		/// <summary>All local drives are scanned.</summary>
		[Description("All Local Drives")]
		All,
		/// <summary>Selected drives are scanned.</summary>
		[Description("Individual Drives")]
		Individual,
		/// <summary>A folder path is scanned.</summary>
		[Description("A Folder")]
		Folder,
	}
}
