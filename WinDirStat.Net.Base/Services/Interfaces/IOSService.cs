using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDirStat.Net.Services {
	/// <summary>A service for OS-specific actions.</summary>
	public interface IOSService {

		#region Privileges

		/// <summary>Gets if the current process has elevated privileges.</summary>
		bool IsElevated { get; }

		/// <summary>Starts a new version of this process in an elevated environment.</summary>
		void StartNewElevated(string arguments = "");

		#endregion

		#region RunItem

		/// <summary>Opens the specified item using the default action.</summary>
		/// 
		/// <param name="file">The file to open.</param>
		void RunItem(string file);

		#endregion

		#region Explore

		/// <summary>Opens the computer folder in Explorer.</summary>
		void ExploreComputer();

		/// <summary>Opens the folder in Explorer.</summary>
		/// 
		/// <param name="folderPath">The path of the folder to open.</param>
		void ExploreFolder(string folderPath);

		/// <summary>Opens the parent folder in Explorer and selects the file.</summary>
		/// 
		/// <param name="filePath">The path of the file to select.</param>
		void ExploreFile(string filePath);

		/// <summary>Opens the properties window for the computer.</summary>
		bool OpenComputerProperties(string filePath);

		/// <summary>Opens the properties window of the file.</summary>
		/// <param name="filePath">The path of the file to view the properties of.</param>
		bool OpenProperties(string filePath);

		#endregion

		#region Console

		/// <summary>Opens the command prompt in the specified working directory.</summary>
		/// 
		/// <param name="directory">The working directory to open the command prompt in.</param>
		void OpenCommandPrompt(string directory);

		/// <summary>Opens PowerShell in the specified working directory.</summary>
		/// 
		/// <param name="directory">The working directory to open PowerShell in.</param>
		void OpenPowerShell(string directory);

		#endregion

		#region Delete/Recycle

		/// <summary>Permanently deletes the file.</summary>
		/// 
		/// <param name="file">The path of the file to delete.</param>
		/// <returns>True if the operation was successful.</returns>
		bool DeleteFile(string file);

		/// <summary>Sends the file or directory to the recycle bin.</summary>
		/// 
		/// <param name="file">The path of the file to delete.</param>
		/// <returns>True if the operation was successful.</returns>
		bool RecycleFile(string file);

		/// <summary>Deletes or recycles the file based on the conditional value.</summary>
		/// 
		/// <param name="file">The path of the file to delete.</param>
		/// <param name="recycle">True if the file should be recycled.</param>
		/// <returns>True if the operation was successful.</returns>
		bool DeleteOrRecycleFile(string file, bool recycle);

		/// <summary>Empties the recycle bin at the specified path.</summary>
		/// 
		/// <param name="windowOwner">The window to own the dialogs.</param>
		/// <param name="path">The path of the recycle bin</param>
		/// <returns>True if the operation was successful.</returns>
		bool EmptyRecycleBin(IWindow owner, string path = "");

		/// <summary>Gets the stats about the specified recycle bin.</summary>
		/// 
		/// <param name="path">The path of the recycle bin</param>
		/// <returns>The info about the recycle bin on success, otherwise null.</returns>
		RecycleBinInfo GetRecycleBinInfo(string path);

		/// <summary>Gets the stats about every recycle bin.</summary>
		/// 
		/// <returns>The info about the recycle bin on success, otherwise null.</returns>
		RecycleBinInfo GetAllRecycleBinInfo();

		#endregion
	}
}
