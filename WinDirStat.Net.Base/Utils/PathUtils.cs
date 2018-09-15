using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WinDirStat.Net.Utils {
	/// <summary>A helper with extra methods for paths, files, and directories.</summary>
	public static class PathUtils {

		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		/// <summary>The stored executable path for the entry assembly.</summary>
		public static readonly string EntryPath =
			Assembly.GetEntryAssembly().Location;

		/// <summary>The directory of the entry executable.</summary>
		public static readonly string EntryDirectory =
			Path.GetDirectoryName(EntryPath);

		/// <summary>Gets the file name of the entry executable.</summary>
		public static readonly string EntryFile =
			Path.GetFileName(EntryPath);

		/// <summary>Gets the file name of the entry executable without its extension.</summary>
		public static readonly string EntryName =
			Path.GetFileNameWithoutExtension(EntryPath);

		/// <summary>
		/// Provides a platform-specific character used to separate directory levels in a path
		/// string that reflects a hierarchical file system organization.
		/// </summary>
		public static readonly char[] DirectorySeparators = {
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar,
		};

		public static readonly string DirectorySeparatorString =
			new string(Path.DirectorySeparatorChar, 1);
		public static readonly string AltDirectorySeparatorString =
			new string(Path.AltDirectorySeparatorChar, 1);
		public static readonly string VolumeSeparatorString =
			new string(Path.VolumeSeparatorChar, 1);

		/// <summary>
		/// Provides a platform-specific character used to separate directory levels in a path
		/// string that reflects a hierarchical file system organization.
		/// </summary>
		public static readonly string[] DotDirectorySeparators = {
			Path.DirectorySeparatorChar + ".",
			Path.AltDirectorySeparatorChar + ".",
		};

		/// <summary>Gets the path to System32 whether this is an x86 or x64 process.</summary>
		/// 
		/// <exception cref="NotSupportedException">
		/// The operating system is not running Windows.
		/// </exception>
		public static string System32 {
			get {
				return Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.Windows),
						Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess
						? "Sysnative" : "System32");
			}
		}

		//-----------------------------------------------------------------------------
		// File Validity
		//-----------------------------------------------------------------------------

		/// <summary>Returns true if the file name has valid characters.</summary>
		public static bool IsValidName(string name) {
			return name.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
		}

		/// <summary>
		/// Returns true if the file name has valid characters for a search pattern.
		/// </summary>
		public static bool IsValidNamePattern(string name) {
			name = name.Replace("*", "").Replace("?", "");
			return IsValidName(name);
		}

		/// <summary>Returns true if the file path has valid characters.</summary>
		public static bool IsValidPath(string path) {
			try {
				Path.GetFullPath(path);
				return true;
			}
			catch {
				return false;
			}
			//return name.IndexOfAny(Path.GetInvalidPathChars()) == -1;
		}

		/// <summary>
		/// Returns true if the file path has valid characters and is not rooted.
		/// </summary>
		public static bool IsValidRelativePath(string path) {
			if (Path.IsPathRooted(path))
				return false;
			return IsValidPath(path);
		}

		/// <summary>
		/// Returns true if the file path has valid characters for a search pattern.
		/// </summary>
		public static bool IsValidPathPattern(string path) {
			path = path.Replace("*", "").Replace("?", "");
			return IsValidPath(path);
			//return path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
		}

		/// <summary>
		/// Returns true if the file path has valid characters and does not lead to a directory.
		/// </summary>
		public static bool IsValidFile(string path) {
			return IsValidPath(path) && !Directory.Exists(path);
		}

		/// <summary>
		/// Returns true if the file path has valid characters, is not rooted. and does not lead to
		/// a directory.
		/// </summary>
		public static bool IsValidRelativeFile(string path) {
			if (Path.IsPathRooted(path))
				return false;
			return IsValidFile(path);
		}

		/// <summary>
		/// Returns true if the file path has valid characters and does not lead to a file.
		/// </summary>
		public static bool IsValidDirectory(string path) {
			return IsValidPath(path) && !File.Exists(path);
		}

		/// <summary>
		/// Returns true if the file path has valid characters, is not rooted. and does not lead to
		/// a file.
		/// </summary>
		public static bool IsValidRelativeDirectory(string path) {
			if (Path.IsPathRooted(path))
				return false;
			return IsValidDirectory(path);
		}


		//-----------------------------------------------------------------------------
		// File or Directory
		//-----------------------------------------------------------------------------

		/// <summary>Copies a file or directory.</summary>
		public static void CopyFileOrDirectory(string sourcePath, string destPath, bool overwrite) {
			if (Directory.Exists(sourcePath))
				CopyDirectory(sourcePath, destPath, overwrite);
			else
				File.Copy(sourcePath, destPath, overwrite);
		}

		/// <summary>Moves a file or directory.</summary>
		public static void MoveFileOrDirectory(string sourcePath, string destPath) {
			if (Directory.Exists(sourcePath))
				Directory.Move(sourcePath, destPath);
			else
				File.Move(sourcePath, destPath);
		}

		/// <summary>Deletes a file or directory.</summary>
		public static void DeleteFileOrDirectory(string path) {
			if (Directory.Exists(path))
				Directory.Delete(path, true);
			else
				File.Delete(path);
		}

		/// <summary>Returns true if a file or directory exists at the path.</summary>
		public static bool Exists(string path) {
			return (File.Exists(path) || Directory.Exists(path));
		}

		/// <summary>Copies the directory and all subfolders and files.</summary>
		public static void CopyDirectory(string sourceDir, string destDir, bool merge) {
			// Get the subdirectories for the specified directory.
			DirectoryInfo dir = new DirectoryInfo(sourceDir);

			if (!dir.Exists)
				throw new DirectoryNotFoundException($"Source directory does not exist or could " +
					$"not be found: {sourceDir}");

			DirectoryInfo[] dirs = dir.GetDirectories();
			// If the destination directory doesn't exist, create it.
			if (File.Exists(destDir))
				throw new IOException("File already exists at location!");
			else if (!Directory.Exists(destDir))
				Directory.CreateDirectory(destDir);
			else if (!merge)
				throw new IOException("Directory already exists at location!");

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = dir.GetFiles();
			foreach (FileInfo file in files) {
				string temppath = Path.Combine(destDir, file.Name);
				file.CopyTo(temppath, true);
			}

			// Copy subdirectories and their contents to new location.
			foreach (DirectoryInfo subdir in dirs) {
				string temppath = Path.Combine(destDir, subdir.Name);
				CopyDirectory(subdir.FullName, temppath, true);
			}
		}


		//-----------------------------------------------------------------------------
		// File Paths
		//-----------------------------------------------------------------------------

		/// <summary>Returns a path that can be compared with another normalized path.</summary>
		public static string NormalizePath(string path) {
			return TrimSeparatorDotEnd(
				Path.GetFullPath(path)
					.TrimEnd(DirectorySeparators)
					.ToUpperInvariant());
		}

		/// <summary>Returns true if the two paths lead to the same location.</summary>
		public static bool IsSamePath(string path1, string path2) {
			return string.Compare(NormalizePath(path1), NormalizePath(path2), true) == 0;
		}

		/// <summary>Removes the ending directory separator from the path.</summary>
		public static string TrimSeparatorEnd(string path) {
			return path.TrimEnd(DirectorySeparators);
		}

		/// <summary>Removes the ending directory separator from the path.</summary>
		public static string TrimSeparatorDotEnd(string path) {
			for (int i = 0; i < DotDirectorySeparators.Length; i++) {
				if (path.EndsWith(DotDirectorySeparators[i]))
					return path.Substring(0, path.Length - DotDirectorySeparators[i].Length);
			}
			return TrimSeparatorEnd(path);
		}

		private static readonly string DirectorySeparatorCharAsString = new string(Path.DirectorySeparatorChar, 1);

		public static string CombineNoChecks(string path1, string path2) {
			if (path2.Length == 0)
				return path1;

			if (path1.Length == 0)
				return path2;

			if (Path.IsPathRooted(path2))
				return path2;

			char ch = path1[path1.Length - 1];
			if (!IsDirectorySeparator(ch))
				return path1 + DirectorySeparatorString + path2;
			return path1 + path2;
		}

		public static string AddLongPathPrefix(string path) {
			if (!path.StartsWith(@"\\?\"))
				return @"\\?\" + path;
			return path;
		}

		/// <summary>
		/// A version of <see cref="Path.GetFullPath(string)"/> that doesn't access the file system.
		/// </summary>
		/// 
		/// <param name="path">The path to expand.</param>
		/// <param name="expandEnvVars">True if environment variables should be expanded.</param>
		/*public static string GetFullPath(string path) {
			bool notRooted = !Path.IsPathRooted(path);
			string currentDirectory = "";
			if (notRooted)
				currentDirectory = Directory.GetDirectories()
			string root = ()

			string dirs = path;
			//string name = Path.GetFileName(path);
			string outPath = "";
			while (dirs.Length != 0) {
				string name = Path.GetFileName(dirs);
				if (name == ".") {

				}

				dirs = Path.GetDirectoryName(dirs);
			}

			if (expandEnvVars && path.Contains("%"))
				path = Environment.ExpandEnvironmentVariables(path);
		}*/

		/*public static IEnumerable<string> EnumerateDirectoryNames(string path) {
			while (path.Contains(DirectorySeparators[0]) || path.Contains(DirectorySeparators[1])) {

			}
		}*/

		public static bool IsPathRoot(string path) {
			return IsSamePath(Path.GetPathRoot(path), path);
		}

		public static string GetRootOrFileName(string path) {
			if (IsPathRoot(path))
				return path;
			else
				return Path.GetFileName(path);
		}

		/// <summary>Returns a file path with " - Copy" appended to the filename.</summary>
		public static string GetCopyName(string path) {
			string newPath = Path.GetFileNameWithoutExtension(path) + " - Copy";
			string ext = Path.GetExtension(path);

			int index = 1;
			string finalPath = newPath + ext;
			while (Exists(finalPath)) {
				index++;
				finalPath = $"{newPath} ({index}){ext}";
			}
			return finalPath;
		}

		/// <summary>Combines the specified paths with the executable directory.</summary>
		public static string CombineExe(string path1) {
			return Path.Combine(EntryDirectory, path1);
		}

		/// <summary>Combines the specified paths with the executable directory.</summary>
		public static string CombineExe(string path1, string path2) {
			return Path.Combine(EntryDirectory, path1, path2);
		}

		/// <summary>Combines the specified paths with the executable directory.</summary>
		public static string CombineExe(string path1, string path2,
			string path3) {
			return Path.Combine(EntryDirectory, path1, path2, path3);
		}

		/// <summary>Combines the specified paths with the executable directory.</summary>
		public static string CombineExe(params string[] paths) {
			return Path.Combine(EntryDirectory, Path.Combine(paths));
		}

		public static string GetTempDirectory(params string[] paths) {
			return Path.Combine(Path.GetTempPath(), Path.Combine(paths));
		}

		public static string CreateTempDirectory(params string[] paths) {
			string path = GetTempDirectory(paths);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			return path;
		}

		public static void CleanupTempDirectory(bool deleteRoot, params string[] paths) {
			string path = GetTempDirectory(paths);

			if (Directory.Exists(path)) {

				if (deleteRoot)
					Directory.Delete(path);
			}
		}

		/// <summary>
		/// Gets the exact case used on the file system for an existing file or directory name.
		/// </summary>
		/// 
		/// <param name="path">A relative or absolute path.</param>
		/// <returns>The name using the correct case if the path exists.</returns>
		/// 
		/// <remarks>
		/// This supports drive-lettered paths and UNC paths, but a UNC root will be returned in
		/// title case (e.g., \\Server\Share).<para/>
		/// <a href="https://stackoverflow.com/a/29578292/7517185">Source</a>
		/// </remarks>
		public static string GetExactName(string path) {
			// DirectoryInfo accepts either a file path or a directory path, and most
			// of its properties work for either. However, its Exists property only
			// works for a directory path.
			DirectoryInfo directory = new DirectoryInfo(path);
			if (File.Exists(path) || directory.Exists) {
				DirectoryInfo parentDirectory = directory.Parent;
				FileSystemInfo child = parentDirectory.EnumerateFileSystemInfos(
						directory.Name).First();
				return child.Name;
			}
			else {
				throw new FileNotFoundException(path);
			}
		}

		/// <summary>
		/// Gets the exact case used on the file system for an existing file or directory path.
		/// </summary>
		/// 
		/// <param name="path">A relative or absolute path.</param>
		/// <returns>The full path using the correct case if the path exists.</returns>
		/// 
		/// <remarks>
		/// This supports drive-lettered paths and UNC paths, but a UNC root will be returned in
		/// title case (e.g., \\Server\Share).<para/>
		/// <a href="https://stackoverflow.com/a/29578292/7517185">Source</a>
		/// </remarks>
		public static string GetExactPath(string path) {
			// DirectoryInfo accepts either a file path or a directory path, and most
			// of its properties work for either. However, its Exists property only
			// works for a directory path.
			DirectoryInfo directory = new DirectoryInfo(path);
			if (File.Exists(path) || directory.Exists) {
				List<string> parts = new List<string>();

				DirectoryInfo parentDirectory = directory.Parent;
				while (parentDirectory != null) {
					FileSystemInfo entry = parentDirectory.EnumerateFileSystemInfos(
						directory.Name).First();
					parts.Add(entry.Name);

					directory = parentDirectory;
					parentDirectory = directory.Parent;
				}

				// Handle the root part (i.e., drive letter or UNC \\server\share).
				string root = directory.FullName;
				if (root.Contains(':')) {
					// Drive Letter
					root = root.ToUpper();
				}
				else {
					// UNC
					string[] rootParts = root.Split(Path.DirectorySeparatorChar);
					root = string.Join(DirectorySeparatorString, rootParts.Select(part =>
						CultureInfo.CurrentCulture.TextInfo.ToTitleCase(part)));
				}

				parts.Add(root);
				parts.Reverse();
				return Path.Combine(parts.ToArray());
			}
			else {
				throw new FileNotFoundException(path);
			}
		}

		public static bool EndsWithSeparator(string path) {
			return path.Length > 0 && IsSeparator(path[path.Length - 1]);
		}
		public static bool EndsWithDirectorySeparator(string path) {
			return path.Length > 0 && IsDirectorySeparator(path[path.Length - 1]);
		}
		public static bool EndsWithVolumeSeparator(string path) {
			return path.Length > 0 && IsVolumeSeparator(path[path.Length - 1]);
		}
		public static string AddVolumeSeparator(string path) {
			if (!EndsWithVolumeSeparator(path))
				return path + Path.VolumeSeparatorChar;
			return path;
		}
		public static string AddDirectorySeparator(string path) {
			if (!EndsWithDirectorySeparator(path))
				return path + Path.DirectorySeparatorChar;
			return path;
		}

		public static bool IsSeparator(char c) {
			return	c == Path.DirectorySeparatorChar ||
					c == Path.AltDirectorySeparatorChar ||
					c == Path.VolumeSeparatorChar;
		}
		public static bool IsDirectorySeparator(char c) {
			return	c == Path.DirectorySeparatorChar ||
					c == Path.AltDirectorySeparatorChar;
		}
		public static bool IsVolumeSeparator(char c) {
			return  c == Path.VolumeSeparatorChar;
		}


		//-----------------------------------------------------------------------------
		// Get Files
		//-----------------------------------------------------------------------------

		#region Get/EnumerateAllFiles

		/// <summary>Returns an array of all files and subfiles in the directory.</summary>
		public static string[] GetAllFiles(string path) {
			return Directory.GetFiles(path, "*", SearchOption.AllDirectories);
		}

		/// <summary>
		/// Returns an array of all files and subfiles in the directory that match the search
		/// pattern.
		/// </summary>
		public static string[] GetAllFiles(string path, string pattern) {
			return Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
		}

		/// <summary>Enumerates all files and subfiles in the directory.</summary>
		public static IEnumerable<string> EnumerateAllFiles(string path) {
			return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
		}

		/// <summary>
		/// Enumerates all files and subfiles in the directory that match the search pattern.
		/// </summary>
		public static IEnumerable<string> EnumerateAllFiles(string path, string pattern) {
			return Directory.EnumerateFiles(path, pattern, SearchOption.AllDirectories);
		}

		#endregion

		//-----------------------------------------------------------------------------
		// Directory
		//-----------------------------------------------------------------------------

		/// <summary>Returns true if the directory has no files in it.</summary>
		public static bool IsDirectoryEmpty(string directory) {
			return !Directory.EnumerateFileSystemEntries(directory).Any();
		}

		/// <summary>
		/// Deletes all empty directories and subdirectories within this directory.
		/// </summary>
		public static void DeleteAllEmptyDirectories(string directory) {
			foreach (string dir in Directory.EnumerateDirectories(directory)) {
				if (IsDirectoryEmpty(dir)) {
					Directory.Delete(dir);
				}
				else {
					DeleteAllEmptyDirectories(dir);
					if (IsDirectoryEmpty(dir))
						Directory.Delete(dir);
				}
			}
		}
	}
}
