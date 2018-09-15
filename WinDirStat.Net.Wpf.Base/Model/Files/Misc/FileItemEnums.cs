using System;
using System.ComponentModel;
using System.IO;

namespace WinDirStat.Net.Model.Files {
	/// <summary>The different types of file tree items.</summary>
	[Serializable]
	public enum FileItemType : byte {
		/// <summary>This item is the computer that stores all scanned volumes.</summary>
		Computer = 0,
		/// <summary>This item is a volume that contains the entire file tree.</summary>
		Volume = 1,
		/// <summary>
		/// This item is a directory that stores its files and subdirs. It can also be a root type.
		/// </summary>
		Directory = 2,
		/// <summary>This item is a normal file and is treated as a treemap leaf.</summary>
		File = 3,
		/// <summary>
		/// This item is a container for files when a folder is storing files and folders.<para/>
		/// See <see cref="FolderItem"/>'s remarks for how files and folders are separated.
		/// </summary>
		FileCollection = 4,
		/// <summary>This item is the free space for a root. This may optionally be hidden.</summary>
		FreeSpace = 5,
		/// <summary>This item is the unidentified space for a root. This may optionally be hidden.</summary>
		Unknown = 6,
	}

	/// <summary>The modifiable states of a file tree item.</summary>
	[Serializable]
	[Flags]
	internal enum FileItemStates : byte {
		/// <summary>No states are set.</summary>
		None = 0,
		
		/// <summary>The container is done scanning all subdirs.</summary>
		Done = (1 << 0),
		/// <summary>
		/// The container is invalidated and needs to have its size and other members validated when needed.
		/// </summary>
		Invalidated = (1 << 1),
		/// <summary>
		/// The container is in the process of validating, this prevents more than one validation operation
		/// from occurring at the same time.
		/// </summary>
		Validating = (1 << 2),
		/// <summary>The file still exists in the system.</summary>
		Exists = (1 << 3),
	}

	/// <summary>The constant flags of a file tree item.</summary>
	[Serializable]
	[Flags]
	internal enum FileItemFlags : ushort {
		/// <summary>No flags are set.</summary>
		None = 0,

		// Sortable FileAttributes (Bit order is important here)
		/// <summary>This file has the <see cref="FileAttributes.Encrypted"/> attribute.</summary>
		Encrypted = (1 << 0),
		/// <summary>This file has the <see cref="FileAttributes.Compressed"/> attribute.</summary>
		Compressed = (1 << 1),
		/// <summary>This file has the <see cref="FileAttributes.Archive"/> attribute.</summary>
		Archive = (1 << 2),
		/// <summary>This file has the <see cref="FileAttributes.System"/> attribute.</summary>
		System = (1 << 3),
		/// <summary>This file has the <see cref="FileAttributes.Hidden"/> attribute.</summary>
		Hidden = (1 << 4),
		/// <summary>This file has the <see cref="FileAttributes.ReadOnly"/> attribute.</summary>
		ReadOnly = (1 << 5),

		// Other FileAttributes
		/// <summary>This file has the <see cref="FileAttributes.ReparsePoint"/> attribute.</summary>
		ReparsePoint = (1 << 6),
		/// <summary>This file has the <see cref="FileAttributes.Temporary"/> attribute.</summary>
		Temporary = (1 << 7),

		// Type Flags
		/// <summary>The directory is case-sensitive.</summary>
		CaseSensitive = (1 << 11),
		/// <summary>The item can contain children.</summary>
		ContainerType = (1 << 12),
		/// <summary>The item is a real file, directory, or volume.</summary>
		FileType = (1 << 13),
		/// <summary>The item is a root and a volume or directory.</summary>
		FileRootType = (1 << 14),
		/// <summary>
		/// The item is the top level root. This can be set at the same time as <see cref="FileRootType"/>.
		/// </summary>
		AbsoluteRootType = (1 << 15),

		/// <summary>The mask for all type flags.</summary>
		[Browsable(false)]
		TypeFlagsMask = 0xF800,
		/// <summary>The mask for all <see cref="FileAttributes"/> used in sorting.</summary>
		[Browsable(false)]
		SortAttributesMask = 0x003F,
		/// <summary>The mask for all <see cref="FileAttributes"/>.</summary>
		[Browsable(false)]
		AttributesMask = 0x00FF,
	}

	/// <summary>
	/// Extensions for settings individual flags in <see cref="FileItemStates"/> and <see cref=
	/// "FileItemFlags"/>.
	/// </summary>
	internal static class FileItemFlagsExtensions {
		/// <summary>Enables or disables the specified flag.</summary>
		/// 
		/// <param name="flags">The flags to modify</param>
		/// <param name="flag">The flag(s) to set.</param>
		/// <param name="value">True if the flags should be set</param>
		/// <returns>The modified flags.</returns>
		public static FileItemStates SetFlag(this FileItemStates flags, FileItemStates flag, bool value) {
			if (value)
				return flags | flag;
			else
				return flags & ~flag;
		}

		/// <summary>Enables or disables the specified flag.</summary>
		/// 
		/// <param name="flags">The flags to modify</param>
		/// <param name="flag">The flag(s) to set.</param>
		/// <param name="value">True if the flags should be set</param>
		/// <returns>The modified flags.</returns>
		public static FileItemFlags SetFlag(this FileItemFlags flags, FileItemFlags flag, bool value) {
			if (value)
				return flags | flag;
			else
				return flags & ~flag;
		}
	}
}
