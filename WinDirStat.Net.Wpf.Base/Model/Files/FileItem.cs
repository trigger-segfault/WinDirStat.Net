using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDirStat.Net.Model.Extensions;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.Model.Files {
	/// <summary>The file tree item that identifies an actual file.</summary>
	[Serializable]
	public class FileItem : FileItemBase {

		#region Fields

		/// <summary>Gets the extension item of the file.</summary>
		public override sealed ExtensionItem ExtensionItem { get; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="FileItem"/> with a <see cref="FileSystemInfo"/>.</summary>
		/// 
		/// <param name="info">The file information.</param>
		/// <param name="extension">The extension information.</param>
		public FileItem(FileSystemInfo info, ExtensionItem extension)
			: base(info, FileItemType.File, FileItemFlags.FileType)
		{
			Debug.Assert(extension != null);
			ExtensionItem = extension;
		}

		/// <summary>Constructs the <see cref="FileItem"/> with a <see cref="IScanFileInfo"/>.</summary>
		/// 
		/// <param name="info">The file information.</param>
		/// <param name="extension">The extension information.</param>
		public FileItem(IScanFileInfo info, ExtensionItem extension)
			: base(info, FileItemType.File, FileItemFlags.FileType)
		{
			Debug.Assert(extension != null);
			ExtensionItem = extension;
		}

		#endregion

		#region FileItemBase Overrides

		/// <summary>
		/// Gets the extension of the file. Empty extensions are always returned with a '.'.
		/// </summary>
		public override sealed string Extension => ExtensionItem.Extension;

		#endregion

		#region FileItemBase Override Methods

		/// <summary>Refreshes the file. Returns true if it still exists.</summary>
		/// 
		/// <returns>True if the file still exists.</returns>
		public override sealed bool Refresh() {
			FileInfo info = new FileInfo(FullName);
			if (info.Exists && !info.Attributes.HasFlag(FileAttributes.Directory)) {
				Attributes = info.Attributes;

				long oldSize = Size;
				if (!info.Attributes.HasFlag(FileAttributes.ReparsePoint))
					Size = info.Length;
				else
					Size = 0L;
				ExtensionItem.RefreshFile(Size, oldSize);

				LastWriteTimeUtc = info.LastWriteTimeUtc;
				
				return RefreshFinal(true);
			}
			ExtensionItem.RemoveFile(Size);
			return RefreshFinal(false);
		}

		/// <summary>Checks if the file still exists.</summary>
		/// 
		/// <returns>True if the file exists.</returns>
		public override sealed bool CheckExists() => CheckExistsFinal(File.Exists(FullName));

		#endregion
	}
}
