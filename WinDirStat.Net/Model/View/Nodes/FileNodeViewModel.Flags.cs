using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.View.Nodes {
	partial class FileNodeViewModel {

		private FileNodeViewModelFlags flags = FileNodeViewModelFlags.None;

#pragma warning disable 0649, IDE1006
		/*private bool isVisible {
			get => flags.HasFlag(FileNodeViewFlags.Visible);
			set => flags.SetFlag(FileNodeViewFlags.Visible, value);
		}*/
		private bool isHidden {
			get => flags.HasFlag(FileNodeViewModelFlags.Hidden);
			set => flags = flags.SetFlag(FileNodeViewModelFlags.Hidden, value);
		}
		private bool isSelected {
			get => flags.HasFlag(FileNodeViewModelFlags.Selected);
			set => flags = flags.SetFlag(FileNodeViewModelFlags.Selected, value);
		}
		private bool isExpanded {
			get => flags.HasFlag(FileNodeViewModelFlags.Expanded);
			set => flags = flags.SetFlag(FileNodeViewModelFlags.Expanded, value);
		}
		private bool isEditing {
			get => flags.HasFlag(FileNodeViewModelFlags.Editing);
			set => flags = flags.SetFlag(FileNodeViewModelFlags.Editing, value);
		}
		private bool isEventsHooked {
			get => flags.HasFlag(FileNodeViewModelFlags.EventsHooked);
			set => flags = flags.SetFlag(FileNodeViewModelFlags.EventsHooked, value);
		}
#pragma warning restore 0649, IDE1006
	}
}
