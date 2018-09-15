using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Settings {
	/// <summary>How icons are stored and displayed.</summary>
	[Serializable]
	public enum IconCacheMode {
		/// <summary>Don't show icons.</summary>
		[Description("Don't show icons")]
		NoIcons,
		/// <summary>Use default icons (File/Folder/Drive/Computer).</summary>
		[Description("Use default icons")]
		Basic,
		/// <summary>Use file type icons.</summary>
		[Description("Use file type icons")]
		FileType,
		/// <summary>Use individual icons.</summary>
		[Description("Individual icons")]
		Individual,
		/// <summary>Use individual icons with overlays.</summary>
		[Description("Individual icons with overlays")]
		IndividualOverlays,
	}
}
