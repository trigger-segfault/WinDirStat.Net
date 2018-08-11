using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Settings {
	[Serializable]
	public enum TreemapStyle {
		[Description("KDirStat")]
		KDirStatStyle = 0,
		[Description("SequoiaView")]
		SequoiaViewStyle = 1,
		[Description("Simple")]
		SimpleStyle = 2,
	}
}
