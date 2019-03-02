using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using WinDirStat.Net.Services;
using static WinDirStat.Net.Native.Win32;

namespace WinDirStat.Net.Services {
	/// <summary>A service for OS-specific actions.</summary>
	public class OSService {

		#region Fields

		/// <summary>The service for performing UI actions such as dispatcher invoking.</summary>
		private readonly UIService ui;
		/// <summary>True if the current process has elevated privileges.</summary>
		private bool? isElevated;

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="UIService"/>.</summary>
		public OSService(UIService ui) {
			this.ui = ui;
		}

		#endregion

		#region Privileges

		/// <summary>Gets if the current process has elevated privileges.</summary>
		public bool IsElevated {
			get {
				if (!isElevated.HasValue)
					isElevated = WindowsIdentity.GetCurrent().Owner
						.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
				return isElevated.Value;
			}
		}

		public void StartNewElevated() {
			Process current = Process.GetCurrentProcess();
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = current.MainModule.FileName,
				Arguments = string.Join(" ", Environment.GetCommandLineArgs().Select(a => "\"" + a + "\"")),
				WorkingDirectory = Directory.GetCurrentDirectory(),
				Verb = "runas",
			};
			Process.Start(startInfo).Dispose();
		}

		#endregion

		#region RunItem

		/// <summary>Opens the specified item using the default action.</summary>
		/// 
		/// <param name="file">The file to open.</param>
		public void RunItem(string file) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = file,
			};
			Process.Start(startInfo)?.Dispose();
		}

		#endregion

		#region Explore

		/// <summary>Opens the computer folder in Explorer.</summary>
		public void ExploreComputer() {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = "explorer.exe",
				Arguments = "::{20d04fe0-3aea-1069-a2d8-08002b30309d}",
			};
			Process.Start(startInfo)?.Dispose();
		}

		/// <summary>Opens the folder in Explorer.</summary>
		/// 
		/// <param name="folderPath">The path of the folder to open.</param>
		public void ExploreFolder(string folderPath) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = "explorer.exe",
				Arguments = $"\"{folderPath}\"",
			};
			Process.Start(startInfo)?.Dispose();
		}

		/// <summary>Opens the parent folder in Explorer and selects the file.</summary>
		/// 
		/// <param name="filePath">The path of the file to select.</param>
		public void ExploreFile(string filePath) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = "explorer.exe",
				Arguments = $"/select,\"{filePath}\"",
			};
			Process.Start(startInfo)?.Dispose();
		}

		/// <summary>Opens the properties window for the computer.</summary>
		public bool OpenComputerProperties(string filePath) {
			return OpenProperties("::{20d04fe0-3aea-1069-a2d8-08002b30309d}");
		}

		/// <summary>Opens the properties window of the file.</summary>
		/// <param name="filePath">The path of the file to view the properties of.</param>
		public bool OpenProperties(string filePath) {
			ShellExecuteInfo info = new ShellExecuteInfo {
				cbSize = ShellExecuteInfo.CBSize,
				lpVerb = "properties",
				lpFile = filePath,
				nShow = ShowCommands.Show,
				fMask = ShellExecuteMaskFlags.InvokeIDList,
			};
			return ShellExecuteEx(ref info);
		}

		#endregion

		#region Console

		/// <summary>Opens the command prompt in the specified working directory.</summary>
		/// 
		/// <param name="directory">The working directory to open the command prompt in.</param>
		public void OpenCommandPrompt(string directory) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = "cmd.exe",
				WorkingDirectory = directory,
			};
			Process.Start(startInfo)?.Dispose();
		}

		/// <summary>Opens PowerShell in the specified working directory.</summary>
		/// 
		/// <param name="directory">The working directory to open PowerShell in.</param>
		public void OpenPowerShell(string directory) {
			ProcessStartInfo startInfo = new ProcessStartInfo {
				FileName = "powershell.exe",
				WorkingDirectory = directory,
			};
			Process.Start(startInfo)?.Dispose();
		}

		#endregion

		#region Delete/Recycle

		/// <summary>Permanently deletes the file.</summary>
		/// 
		/// <param name="file">The path of the file to delete.</param>
		/// <returns>True if the operation was successful.</returns>
		public bool DeleteFile(string file) {
			return DeleteOrRecycleFile(file, false);
		}

		/// <summary>Sends the file or directory to the recycle bin.</summary>
		/// 
		/// <param name="file">The path of the file to delete.</param>
		/// <returns>True if the operation was successful.</returns>
		public bool RecycleFile(string file) {
			return DeleteOrRecycleFile(file, true);
		}

		/// <summary>Deletes or recycles the file based on the conditional value.</summary>
		/// 
		/// <param name="file">The path of the file to delete.</param>
		/// <param name="recycle">True if the file should be recycled.</param>
		/// <returns>True if the operation was successful.</returns>
		public bool DeleteOrRecycleFile(string file, bool recycle) {
			return ui.Invoke(() => {
				FileOperationFlags flags = FileOperationFlags.None;
				if (recycle)
					flags = FileOperationFlags.AllowUndo | FileOperationFlags.WarnFilesTooBigForRecycleBin;
				SHFileOperationStruct fileOp = new SHFileOperationStruct {
					wFunc = FileOperationFunc.Delete,
					pFrom = file + '\0' + '\0',
					fFlags = flags,
				};
				return !SHFileOperation(ref fileOp);
			});
		}

		/// <summary>Empties the recycle bin at the specified path.</summary>
		/// 
		/// <param name="windowOwner">The window to own the dialogs.</param>
		/// <param name="path">The path of the recycle bin</param>
		/// <returns>True if the operation was successful.</returns>
		public bool EmptyRecycleBin(Window owner, string path = "") {
			return ui.Invoke(() => {
				return !SHEmptyRecycleBin(new WindowInteropHelper(owner).Handle, path, SHEmptyRecycleBinFlags.None);
			});
		}

		/// <summary>Gets the stats about the specified recycle bin.</summary>
		/// 
		/// <param name="path">The path of the recycle bin</param>
		/// <returns>The info about the recycle bin on success, otherwise null.</returns>
		public RecycleBinInfo GetRecycleBinInfo(string path) {
			//return ui.Invoke(() => {
				SHRecycleBinInfo rbInfo = new SHRecycleBinInfo {
					cbSize = SHRecycleBinInfo.CBSize,
				};
				if (!SHQueryRecycleBin(path, ref rbInfo))
					return new RecycleBinInfo(path, rbInfo.i64NumItems, rbInfo.i64Size);
				return null;
			//});
		}

		/// <summary>Gets the stats about every recycle bin.</summary>
		/// 
		/// <returns>The info about the recycle bin on success, otherwise null.</returns>
		public RecycleBinInfo GetAllRecycleBinInfo() {
			//return ui.Invoke(() => {
				long itemCount = 0;
				long size = 0;
				foreach (DriveInfo driveInfo in DriveInfo.GetDrives()) {
					if (driveInfo.IsReady && driveInfo.DriveType != DriveType.Unknown &&
						driveInfo.DriveType != DriveType.NoRootDirectory)
					{
						RecycleBinInfo rbInfo = GetRecycleBinInfo(driveInfo.Name);
						if (rbInfo != null) {
							itemCount += rbInfo.ItemCount;
							size += rbInfo.Size;
						}
					}
				}
				return new RecycleBinInfo(itemCount, size);
			//});
		}

		#endregion
	}
}
