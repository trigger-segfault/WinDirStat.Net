using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinDirStat.Net.Data.Nodes;
using WinDirStat.Net.Drawing;
using WinDirStat.Net.Utils;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
using static WinDirStat.Net.Utils.Native.Win32;

namespace WinDirStat.Net.Data {
	public interface IFileFindData {
		string FullName { get; }
		string Name { get; }
		long Size { get; }
		FileAttributes Attributes { get; }
		DateTime CreationTime { get; }
		DateTime LastAccessTime { get; }
		DateTime LastWriteTime { get; }
		bool IsDirectory { get; }
	}

	internal partial class FileEnumerator {

		private struct FolderState {
			public FolderNode Folder;
			public ScanState State;

			public FolderState(ScanState state) {
				Folder = state.Root;
				State = state;
			}

			public FolderState(FolderNode folder, ScanState state) {
				State = state;
				Folder = folder;
			}
		}

		static readonly IntPtr InvalidHandle = new IntPtr(-1);


		private void ScanNative(Action<RootNode> rootSetup, CancellationToken token) {
			//root = new RootNode(document, new DirectoryInfo(rootPath), true);
			//SetupRoot(rootSetup);
			Queue<FolderState> subdirs = new Queue<FolderState>();
			foreach (ScanState state in states)
				subdirs.Enqueue(new FolderState(state));
			do {
				SuspendCheck();
				ReadNative(subdirs, token);
			} while (subdirs.Any() && !token.IsCancellationRequested);
		}

		private void ReadNative(Queue<FolderState> subdirs, CancellationToken token) {
			Win32FindData fData = new Win32FindData();
			FolderState folderState = subdirs.Dequeue();
			ScanState state = folderState.State;
			FolderNode parent = folderState.Folder;
			bool findResult;
			string parentPath = parent.Path;
			string searchPattern = PathUtils.CombineNoChecks(parentPath, "*");
			if (!searchPattern.StartsWith(@"\\?\"))
				searchPattern = @"\\?\" + searchPattern;
			IntPtr hFind = FindFirstFileEx(searchPattern, FindExInfoLevels.Basic, out fData, FindExSearchOps.NameMatch, IntPtr.Zero, FindExFlags.LargeFetch);
			if (hFind == InvalidHandle)
				return;

			FolderNode fileCollection = null;
			try {
				do {
					string filePath = PathUtils.CombineNoChecks(parentPath, fData.cFileName);
					if (fData.IsRelativeDirectory || SkipFile(state, fData.cFileName, filePath)) {
						// Skip these types of entries
						findResult = FindNextFile(hFind, out fData);
						continue;
					}
					FileFindData data = new FileFindData(fData, filePath);
					FileNodeBase child;
					if (data.IsDirectory) {
						FolderNode folder = new FolderNode(data);
						child = folder;
						subdirs.Enqueue(new FolderState(folder, state));
					}
					else {
						FileNode file = new FileNode(data);
						child = file;
						if (!fData.IsSymbolicLink) {
							state.ScannedSize += child.Size;
							totalScannedSize += child.Size;
						}
						file.extRecord = document.Extensions.Include(GetExtension(file.Name), child.Size);
					}
					parent.AddChild(state.Root, child, ref fileCollection);

					SuspendCheck();
					if (token.IsCancellationRequested)
						return;

					findResult = FindNextFile(hFind, out fData);
				} while (findResult);

				if (parent.IsExpanded) {
					Application.Current.Dispatcher.Invoke(() => {
						parent.Validate(state.Root, false);
					});
				}
			}
			finally {
				FindClose(hFind);
			}
		}

		private class FileFindData : IFileFindData {
			public string FullName { get; }
			public string Name {
				get => Path.GetFileName(FullName);
			}
			public long Size { get; }
			public FileAttributes Attributes { get; }
			public DateTime CreationTime { get; }
			public DateTime LastAccessTime { get; }
			public DateTime LastWriteTime { get; }
			public bool IsDirectory {
				get => Attributes.HasFlag(FileAttributes.Directory);
			}

			public FileFindData(Win32FindData fData, string path) {
				FullName = path;
				Size = ((long) fData.nFileSizeHigh << 32) | fData.nFileSizeLow;
				Attributes = fData.dwFileAttributes;
				CreationTime = ToDateTime(fData.ftCreationTime);
				LastAccessTime = ToDateTime(fData.ftLastAccessTime);
				LastWriteTime = ToDateTime(fData.ftLastWriteTime);
			}

			private static DateTime ToDateTime(FILETIME fileTime) {
				long ticks = ((long) fileTime.dwHighDateTime << 32) | unchecked((uint) fileTime.dwLowDateTime);
				return DateTime.FromFileTimeUtc(ticks);
			}
		}

		/*[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct Win32FindData {
			[MarshalAs(UnmanagedType.U4)]
			public FileAttributes dwFileAttributes;
			public FILETIME ftCreationTime;
			public FILETIME ftLastAccessTime;
			public FILETIME ftLastWriteTime;
			public int nFileSizeHigh;
			public uint nFileSizeLow;
			public uint dwReserved0;
			public uint dwReserved1;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string cFileName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			private string cAlternateFileName;
			
			public bool IsSymbolicLink {
				get => dwFileAttributes.HasFlag(FileAttributes.ReparsePoint);
			}
			internal bool IsRelativeDirectory {
				get => cFileName == "." || cFileName == "..";
			}

			private static DateTime ToDateTime(FILETIME fileTime) {
				long ticks = ((long) fileTime.dwHighDateTime << 32) | unchecked((uint) fileTime.dwLowDateTime);
				return DateTime.FromFileTimeUtc(ticks);
			}
		}

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr FindFirstFileEx(
			string lpFileName,
			FindExInfoLevels fInfoLevelId,
			out Win32FindData lpFindFileData,
			FindExSearchOps fSearchOp,
			IntPtr lpSearchFilter,
			[MarshalAs(UnmanagedType.U4)] FindExFlags dwAdditionalFlags);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool FindNextFile(
			IntPtr hFindFile,
			out Win32FindData lpFindFileData);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool FindClose(
			IntPtr hFindFile);

		private enum FindExInfoLevels {
			/// <summary>
			/// The FindFirstFileEx function retrieves a standard set of attribute information. The data is
			/// returned in a <see cref="Win32FindData"/> structure.
			/// </summary>
			Standard,
			/// <summary>
			/// The FindFirstFileEx function does not query the short file name, improving overall
			/// enumeration speed.The data is returned in a WIN32_FIND_DATA structure, and the
			/// cAlternateFileName member is always a NULL string. Windows Server 2008, Windows Vista,
			/// Windows Server 2003 and Windows XP:  This value is not supported until Windows Server 2008 R2
			/// and Windows 7.
			/// </summary>
			Basic,
			[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
			MaxInfoLevel,
		}
		private enum FindExSearchOps {
			/// <summary>
			/// The search for a file that matches a specified file name. The lpSearchFilter parameter of
			/// <see cref="FindFirstFileEx"/> must be NULL when this search operation is used.
			/// </summary>
			NameMatch,
			/// <summary>
			/// This is an advisory flag. If the file system supports directory filtering, the function
			/// searches for a file that matches the specified name and is also a directory. If the file
			/// system does not support directory filtering, this flag is silently ignored. The
			/// lpSearchFilter parameter of the FindFirstFileEx function must be NULL when this search value
			/// is used. If directory filtering is desired, this flag can be used on all file systems, but
			/// because it is an advisory flag and only affects file systems that support it, the application
			/// must examine the file attribute data stored in the lpFindFileData parameter of the <see cref=
			/// "FindFirstFileEx"/> function to determine whether the function has returned a handle to a
			/// directory.
			/// </summary>
			LimitToDirectories,
			/// <summary>This filtering type is not available.</summary>
			LimitToDevices,
			[Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
			MaxSearchOp,
		}
		private enum FindExFlags : uint {
			/// <summary>No flags.</summary>
			None = 0,
			/// <summary>Searches are case-sensitive.</summary>
			CaseSensitive = 1,
			/// <summary>
			/// Uses a larger buffer for directory queries, which can increase performance of the find
			/// operation. Windows Server 2008, Windows Vista, Windows Server 2003 and Windows XP: This
			/// value is not supported until Windows Server 2008 R2 and Windows 7.
			/// </summary>
			LargeFetch = 2,
			/// <summary>
			/// Limits the results to files that are physically on disk. This flag is only relevant when a
			/// file virtualization filter is present. 
			/// </summary>
			OnDiskEntriesOnly = 4,
		}*/
	}
}
