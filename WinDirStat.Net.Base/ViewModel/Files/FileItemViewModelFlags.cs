using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.ViewModel.Files {
	/// <summary>Flags for the <see cref="FileItemViewModel"/> state.</summary>
	[Serializable]
	[Flags]
	public enum FileItemViewModelFlags : byte {
		/// <summary>No flags.</summary>
		None = 0,

		//Visible = (1 << 0),
		/// <summary>True if the item is hidden.</summary>
		Hidden = (1 << 0),
		/// <summary>True if the item is selected.</summary>
		Selected = (1 << 1),
		/// <summary>True if the item is expanded.</summary>
		Expanded = (1 << 2),
		/// <summary>True if the item is being edited.</summary>
		Editing = (1 << 3),
		/// <summary>True if the item has its events hooked to the model.</summary>
		EventsHooked = (1 << 4),
		/// <summary>True if the item is waiting on the extension's icon to cache.</summary>
		WaitingForExtensionIcon = (1 << 5),
		/// <summary>The file still exists in the system.</summary>
		Exists = (1 << 6),

	}

	/// <summary>Extensions methods for <see cref="FileItemViewModelFlags"/>.</summary>
	internal static class FileItemViewModelExtensions {
		
		public static FileItemViewModelFlags SetFlag(this FileItemViewModelFlags flags, FileItemViewModelFlags flag, bool value) {
			if (value)
				return flags | flag;
			else
				return flags & ~flag;
		}
	}
}
