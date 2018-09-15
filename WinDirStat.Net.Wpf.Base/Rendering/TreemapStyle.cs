using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Rendering {
	/// <summary>The types of render methods for treemaps.</summary>
	[Serializable]
	public enum TreemapStyle {
		/// <summary>Horizontal/Vertical align.</summary>
		[Description("KDirStat")]
		KDirStatStyle = 0,
		/// <summary>Corner align.</summary>
		[Description("SequoiaView")]
		SequoiaViewStyle = 1,
		/// <summary>Dunno.</summary>
		[Description("Simple")]
		SimpleStyle = 2,
	}
}
