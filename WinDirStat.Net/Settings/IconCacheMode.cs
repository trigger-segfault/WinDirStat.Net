using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Settings {
	public enum IconCacheMode {
		[Description("Don't show icons")]
		NoIcons,
		[Description("Use default icons")]
		Basic,
		[Description("Use file type icons")]
		FileType,
		[Description("Individual icons")]
		Individual,
		[Description("Individual icons with overlays")]
		IndividualOverlays,
	}
}
