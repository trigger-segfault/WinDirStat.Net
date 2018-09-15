using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.ViewModel.Files {
	partial class FileItemViewModel {

		private FileItemViewModelFlags flags = FileItemViewModelFlags.None;

#pragma warning disable 0649, IDE1006
		/*private bool isVisible {
			get => flags.HasFlag(FileNodeViewFlags.Visible);
			set => flags.SetFlag(FileNodeViewFlags.Visible, value);
		}*/
		private bool isHidden {
			get => flags.HasFlag(FileItemViewModelFlags.Hidden);
			set => flags = flags.SetFlag(FileItemViewModelFlags.Hidden, value);
		}
		private bool isSelected {
			get => flags.HasFlag(FileItemViewModelFlags.Selected);
			set => flags = flags.SetFlag(FileItemViewModelFlags.Selected, value);
		}
		private bool isExpanded {
			get => flags.HasFlag(FileItemViewModelFlags.Expanded);
			set => flags = flags.SetFlag(FileItemViewModelFlags.Expanded, value);
		}
		private bool isEditing {
			get => flags.HasFlag(FileItemViewModelFlags.Editing);
			set => flags = flags.SetFlag(FileItemViewModelFlags.Editing, value);
		}
		private bool isEventsHooked {
			get => flags.HasFlag(FileItemViewModelFlags.EventsHooked);
			set => flags = flags.SetFlag(FileItemViewModelFlags.EventsHooked, value);
		}
		private bool isWaitingForExtensionIcon {
			get => flags.HasFlag(FileItemViewModelFlags.WaitingForExtensionIcon);
			set => flags = flags.SetFlag(FileItemViewModelFlags.WaitingForExtensionIcon, value);
		}
		private bool exists {
			get => flags.HasFlag(FileItemViewModelFlags.Exists);
			set => flags = flags.SetFlag(FileItemViewModelFlags.Exists, value);
		}
#pragma warning restore 0649, IDE1006
	}
}
