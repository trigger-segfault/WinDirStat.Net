using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Model.Files {
	/// <summary>An interface revealing information needed to construct file tree items.</summary>
	public interface IScanFileInfo {

		#region FileInfo

		/// <summary>Gets the name of the file.</summary>
		string Name { get; }
		/// <summary>Gets the full path of the file.</summary>
		string FullName { get; }
		/// <summary>Gets the size of the file.</summary>
		long Size { get; }
		/// <summary>Gets the attributes of the file.</summary>
		FileAttributes Attributes { get; }
		/// <summary>Gets the UTC creation time of the file.</summary>
		DateTime CreationTimeUtc { get; }
		/// <summary>Gets the UTC last access time of the file.</summary>
		DateTime LastAccessTimeUtc {  get; }
		/// <summary>Gets the UTC last write time of the file.</summary>
		DateTime LastWriteTimeUtc { get; }

		#endregion

		#region Helpers

		/// <summary>Gets if the file is a directory.</summary>
		bool IsDirectory { get; }
		/// <summary>Gets if the file is a symbolic link.</summary>
		bool IsSymbolicLink { get; }

		#endregion
	}
}
