using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Data.Nodes {
	public partial class FileNodeBase {

		private FileNodeType type;
		protected FileNodeStateFlags state;
		private FileNodeFlags flags;
		
		public FileNodeType Type {
			get => type;
			private set {
				type = value;
				switch (value) {
				case FileNodeType.Computer:
				case FileNodeType.Volume:
				case FileNodeType.Directory:
				case FileNodeType.FileCollection:
					flags |= FileNodeFlags.ContainerType;
					break;
				default:
					flags &= ~FileNodeFlags.ContainerType;
					break;
				}
				switch (value) {
				case FileNodeType.Volume:
				case FileNodeType.Directory:
				case FileNodeType.File:
					flags |= FileNodeFlags.FileType;
					break;
				default:
					flags &= ~FileNodeFlags.FileType;
					break;
				}
			}
		}
		public FileAttributes Attributes {
			get {
				FileAttributes attr = 0;
				if (IsReadOnlyFile)		attr |= FileAttributes.ReadOnly;
				if (IsHiddenFile)		attr |= FileAttributes.Hidden;
				if (IsSystemFile)		attr |= FileAttributes.System;
				if (IsArchiveFile)		attr |= FileAttributes.Archive;
				if (IsCompressedFile)	attr |= FileAttributes.Compressed;
				if (IsEncryptedFile)	attr |= FileAttributes.Encrypted;
				if (IsReparsePointFile)	attr |= FileAttributes.ReparsePoint;
				if (IsTemporaryFile)	attr |= FileAttributes.Temporary;
				return attr;
			}
			private set {
				IsReadOnlyFile		= value.HasFlag(FileAttributes.ReadOnly);
				IsHiddenFile		= value.HasFlag(FileAttributes.Hidden);
				IsSystemFile		= value.HasFlag(FileAttributes.System);
				IsArchiveFile		= value.HasFlag(FileAttributes.Archive);
				IsCompressedFile	= value.HasFlag(FileAttributes.Compressed);
				IsEncryptedFile		= value.HasFlag(FileAttributes.Encrypted);
				IsReparsePointFile	= value.HasFlag(FileAttributes.ReparsePoint);
				IsTemporaryFile		= value.HasFlag(FileAttributes.Temporary);
			}
		}
		public FileNodeFlags SortAttributes {
			get => flags & FileNodeFlags.SortAttributesMask;
		}
		public bool IsReadOnlyFile {
			get => flags.HasFlag(FileNodeFlags.ReadOnly);
			private set => flags = flags.SetFlag(FileNodeFlags.ReadOnly, value);
		}
		public bool IsHiddenFile {
			get => flags.HasFlag(FileNodeFlags.Hidden);
			private set => flags = flags.SetFlag(FileNodeFlags.Hidden, value);
		}
		public bool IsSystemFile {
			get => flags.HasFlag(FileNodeFlags.System);
			private set => flags = flags.SetFlag(FileNodeFlags.System, value);
		}
		public bool IsArchiveFile {
			get => flags.HasFlag(FileNodeFlags.Archive);
			private set => flags = flags.SetFlag(FileNodeFlags.Archive, value);
		}
		public bool IsCompressedFile {
			get => flags.HasFlag(FileNodeFlags.Compressed);
			private set => flags = flags.SetFlag(FileNodeFlags.Compressed, value);
		}
		public bool IsEncryptedFile {
			get => flags.HasFlag(FileNodeFlags.Encrypted);
			private set => flags = flags.SetFlag(FileNodeFlags.Encrypted, value);
		}
		public bool IsReparsePointFile {
			get => flags.HasFlag(FileNodeFlags.ReparsePoint);
			private set => flags = flags.SetFlag(FileNodeFlags.ReparsePoint, value);
		}
		public bool IsTemporaryFile {
			get => flags.HasFlag(FileNodeFlags.Temporary);
			private set => flags = flags.SetFlag(FileNodeFlags.Temporary, value);
		}

		/*public bool IsDone {
			get => state.HasFlag(FileNodeStateFlags.Done);
			internal set => state = state.SetFlag(FileNodeStateFlags.Done, value);
		}
		public bool IsInvalidated {
			get => state.HasFlag(FileNodeStateFlags.Invalidated);
			protected set => state = state.SetFlag(FileNodeStateFlags.Invalidated, value);
		}
		public bool IsValidating {
			get => state.HasFlag(FileNodeStateFlags.Validating);
			protected set => state = state.SetFlag(FileNodeStateFlags.Validating, value);
		}
		public bool IsPopulated {
			get => state.HasFlag(FileNodeStateFlags.Populated);
			protected set => state = state.SetFlag(FileNodeStateFlags.Populated, value);
		}
		public bool IsExpanded {
			get => state.HasFlag(FileNodeStateFlags.Populated);
			set {
				state = state.SetFlag(FileNodeStateFlags.Populated, value);
		}*/

		/// <summary>True if this type can contain other nodes.</summary>
		public bool IsContainerType {
			get => flags.HasFlag(FileNodeFlags.ContainerType);
			private set => flags = flags.SetFlag(FileNodeFlags.ContainerType, value);
		}
		/// <summary>True if this is a real file system node.</summary>
		public bool IsFileType {
			get => flags.HasFlag(FileNodeFlags.FileType);
			private set => flags = flags.SetFlag(FileNodeFlags.FileType, value);
		}
		/// <summary>True if this node contains the root path for the file tree.</summary>
		public bool IsFileRootType {
			get => flags.HasFlag(FileNodeFlags.FileRootType);
			private set => flags = flags.SetFlag(FileNodeFlags.FileRootType, value);
		}
		/// <summary>True if this node never has a parent. A computer node is always of this type.</summary>
		public bool IsAbsoluteRootType {
			get => flags.HasFlag(FileNodeFlags.AbsoluteRootType);
			private set => flags = flags.SetFlag(FileNodeFlags.AbsoluteRootType, value);
		}
		/// <summary>True if this node is an absolute or file root.</summary>
		public bool IsAnyRootType {
			get => (flags & (FileNodeFlags.FileRootType | FileNodeFlags.AbsoluteRootType)) != 0;
		}
	}
}
