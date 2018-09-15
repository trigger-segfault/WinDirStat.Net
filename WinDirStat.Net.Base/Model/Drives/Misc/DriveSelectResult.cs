using System;
using WinDirStat.Net.Services;

namespace WinDirStat.Net.Model.Drives {
	/// <summary>The result returned from the drive select dialog.</summary>
	public class DriveSelectResult {

		#region Fields

		/// <summary>The scanning service that contains this collection.</summary>
		private readonly ScanningService scanning;
		/// <summary>
		/// The constant selected paths. Null when Mode is <see cref="DriveSelectMode.All"/>.
		/// </summary>
		private readonly string[] selectedPaths;
		
		/// <summary>The selection mode of the drive select result.</summary>
		public DriveSelectMode Mode { get; }

		#endregion

		#region Constructors

		/// <summary>Constructs the <see cref="DriveSelectResult"/>.</summary>
		public DriveSelectResult(ScanningService scanning,
								 DriveSelectMode mode,
								 string[] selectedDrives,
								 string folderPath)
		{
			this.scanning = scanning;
			Mode = mode;
			if (mode == DriveSelectMode.Individual) {
				if (selectedDrives == null)
					throw new ArgumentNullException(nameof(selectedDrives));
				selectedPaths = selectedDrives;
			}
			else if (mode == DriveSelectMode.Folder) {
				selectedPaths = new[] {
					folderPath ?? throw new ArgumentNullException(nameof(folderPath)),
				};
			}
		}

		#endregion

		#region Accessors

		/// <summary>Gets the result path of the drive select operation.</summary>
		/// 
		/// <returns>Returns the paths of the result.</returns>
		public string[] GetResultPaths() {
			if (Mode == DriveSelectMode.All) {
				return scanning.ScanDriveNames();
			}
			else {
				return selectedPaths;
			}
		}
		
		#endregion
	}
}
