using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Data.Nodes {
	[Serializable]
	[Flags]
	public enum FileNodeType : byte {
		// Type
		Computer = 0,
		Volume = 1,
		Directory = 2,
		File = 3,
		FileCollection = 4,
		FreeSpace = 5,
		Unknown = 6,
	}

	[Serializable]
	[Flags]
	public enum FileNodeStateFlags : byte {
		None = 0,

		// State Flags
		Done = (1 << 0),
		Invalidated = (1 << 1),
		Validating = (1 << 2),
		//Populated = (1 << 3),
		//Expanded = (1 << 4),
	}

	[Serializable]
	[Flags]
	public enum FileNodeFlags : ushort {
		None = 0,
		
		// Attributes
		// Order here is used for sorting attributes
		Encrypted = (1 << 0),
		Compressed = (1 << 1),
		Archive = (1 << 2),
		System = (1 << 3),
		Hidden = (1 << 4),
		ReadOnly = (1 << 5),

		// Other attributes
		ReparsePoint = (1 << 6),
		Temporary = (1 << 7),

		// Type Flags
		ContainerType = (1 << 12),
		FileType = (1 << 13),
		FileRootType = (1 << 14),
		AbsoluteRootType = (1 << 15),

		[Browsable(false)]
		TypeFlagsMask = 0xF000,
		[Browsable(false)]
		SortAttributesMask = 0x003F,
		[Browsable(false)]
		AttributesMask = 0x00FF,
	}
	internal static class FileNodeFlagsExtensions {

		public static FileNodeStateFlags SetFlag(this FileNodeStateFlags flags, FileNodeStateFlags flag, bool value) {
			if (value)
				return flags | flag;
			else
				return flags & ~flag;
		}

		public static FileNodeFlags SetFlag(this FileNodeFlags flags, FileNodeFlags flag, bool value) {
			if (value)
				return flags | flag;
			else
				return flags & ~flag;
		}
	}
}
