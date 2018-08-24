using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.View.Nodes {
	[Serializable]
	[Flags]
	public enum FileNodeViewModelFlags : byte {
		None = 0,

		//Visible = (1 << 0),
		Hidden = (1 << 1),
		Selected = (1 << 2),
		Expanded = (1 << 3),
		Editing = (1 << 4),
		EventsHooked = (1 << 5),
	}

	internal static class FileNodeViewFlagsExtensions {
		
		public static FileNodeViewModelFlags SetFlag(this FileNodeViewModelFlags flags, FileNodeViewModelFlags flag, bool value) {
			if (value)
				return flags | flag;
			else
				return flags & ~flag;
		}
	}
}
